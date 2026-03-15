import { chromium, type Browser, type Page } from "playwright";
import { interactionRegistry } from "../interactions/registry.js";
import { runInteractions } from "../interactions/runner.js";
import {
  installAnimationObserver,
  collectAnimationSamples,
  installDualAnimationObserver,
  collectDualAnimationSamples,
  buildTrace,
  classifyAnimation,
  compareEndState,
  detectRebound,
} from "../extract/animator.js";
import type {
  AnimationReport,
  AnimationTrace,
  InteractionStep,
  SwitchAnimationReport,
} from "../types.js";

interface AnimateCommandOptions {
  refBase?: string;
  testBase?: string;
  timeout?: string;
  format?: "json" | "summary";
  mode?: "close" | "switch";
}

export async function animateCommand(
  component: string,
  options: AnimateCommandOptions
): Promise<void> {
  const refBase = (options.refBase ?? "http://localhost:5200").replace(/\/$/, "");
  const testBase = (options.testBase ?? "http://127.0.0.1:5199").replace(/\/$/, "");
  const timeoutMs = parseInt(options.timeout ?? "500", 10);
  const format = options.format ?? "summary";
  const mode = options.mode ?? "close";

  const steps = interactionRegistry[component];
  if (!steps || steps.length === 0) {
    console.error(
      `No interaction steps for "${component}". ` +
        `Available: ${Object.keys(interactionRegistry).join(", ")}`
    );
    process.exit(2);
  }

  const animStep = steps.find((s) => s.targetSelector);
  if (!animStep) {
    console.error(
      `No targetSelector defined for "${component}". ` +
        `Animation sampling requires a targetSelector in the interaction registry.`
    );
    process.exit(2);
  }

  if (mode === "switch") {
    if (!animStep.switchSelector || !animStep.switchTargetSelector) {
      console.error(
        `No switchSelector/switchTargetSelector defined for "${component}". ` +
          `Switch-mode animation requires these fields in the interaction registry.`
      );
      process.exit(2);
    }
  }

  const browser = await chromium.launch({ headless: true });

  try {
    if (mode === "close") {
      const report = await runCloseMode(
        browser, component, refBase, testBase, steps, animStep, timeoutMs
      );

      if (format === "json") {
        console.log(JSON.stringify(report, null, 2));
      } else {
        outputCloseAnimationSummary(component, report.ref, report.test, report.status as "match" | "diff");
      }

      process.exit(report.status === "match" ? 0 : 1);
    } else {
      const report = await runSwitchMode(
        browser, component, refBase, testBase, steps, animStep, timeoutMs
      );

      if (format === "json") {
        console.log(JSON.stringify(report, null, 2));
      } else {
        outputSwitchAnimationSummary(component, report);
      }

      process.exit(report.status === "match" ? 0 : 1);
    }
  } finally {
    await browser.close();
  }
}

/**
 * Runs close-mode animation comparison and returns the report.
 * Extracted from animateCommand so it can be reused by the verify command.
 */
export async function runCloseMode(
  browser: Browser,
  component: string,
  refBase: string,
  testBase: string,
  steps: InteractionStep[],
  animStep: InteractionStep,
  timeoutMs: number
): Promise<AnimationReport> {
  const refSamples = await captureCloseAnimation(
    browser, `${refBase}/${component}`, false, steps, animStep, timeoutMs
  );
  const refTrace = buildTrace("ref", refSamples);

  const testSamples = await captureCloseAnimation(
    browser, `${testBase}/${component}`, true, steps, animStep, timeoutMs
  );
  const testTrace = buildTrace("test", testSamples);

  const refClass = classifyAnimation(refTrace);
  const testClass = classifyAnimation(testTrace);
  const endState = compareEndState(refTrace, testTrace);

  // Both classification and end-state must match. This catches cases where
  // both traces are "smooth" but ref ends hidden (0px) while test stays
  // at full height (close animation had no visible effect).
  const status = (refClass === testClass && endState.match) ? "match" : "diff";

  // Detect rebound on the test trace only — the React reference never rebounds.
  const rebound = detectRebound(testSamples);

  return { component, mode: "close", status, ref: refTrace, test: testTrace, rebound };
}

/**
 * Runs switch-mode animation comparison and returns the report.
 * Opens item 1, then clicks item 2 and samples both the closing panel 1
 * and the opening panel 2 simultaneously.
 */
export async function runSwitchMode(
  browser: Browser,
  component: string,
  refBase: string,
  testBase: string,
  steps: InteractionStep[],
  animStep: InteractionStep,
  timeoutMs: number
): Promise<SwitchAnimationReport> {
  const refResult = await captureSwitchAnimation(
    browser, `${refBase}/${component}`, false, steps, animStep, timeoutMs
  );
  const testResult = await captureSwitchAnimation(
    browser, `${testBase}/${component}`, true, steps, animStep, timeoutMs
  );

  const refCloseTrace = buildTrace("ref", refResult.closing);
  const refOpenTrace = buildTrace("ref", refResult.opening);
  const testCloseTrace = buildTrace("test", testResult.closing);
  const testOpenTrace = buildTrace("test", testResult.opening);

  const closeClassMatch = classifyAnimation(refCloseTrace) === classifyAnimation(testCloseTrace);
  const closeEndMatch = compareEndState(refCloseTrace, testCloseTrace).match;
  const openClassMatch = classifyAnimation(refOpenTrace) === classifyAnimation(testOpenTrace);
  const openEndMatch = compareEndState(refOpenTrace, testOpenTrace).match;
  const closeMatch = closeClassMatch && closeEndMatch;
  const openMatch = openClassMatch && openEndMatch;
  const status = closeMatch && openMatch ? "match" : "diff";

  return {
    component,
    mode: "switch",
    status,
    closing: { ref: refCloseTrace, test: testCloseTrace },
    opening: { ref: refOpenTrace, test: testOpenTrace },
  };
}

async function captureCloseAnimation(
  browser: Browser,
  url: string,
  blazor: boolean,
  openSteps: InteractionStep[],
  animStep: InteractionStep,
  timeoutMs: number
): Promise<import("../types.js").AnimationSample[]> {
  const page = await browser.newPage();

  try {
    await page.goto(url, { waitUntil: "networkidle" });

    if (blazor) {
      await page.waitForFunction('typeof window.Blazor !== "undefined"', null, {
        timeout: 15_000,
      });
      await page.waitForTimeout(500);
    }

    // Open the component.
    await runInteractions(page, openSteps);

    // Install the animation observer *before* triggering close.
    await installAnimationObserver(page, animStep.targetSelector!);

    // Trigger close via reverse action.
    const reverseAction = animStep.reverseAction ?? animStep.action;
    const reverseSelector = animStep.reverseSelector ?? animStep.selector;
    await triggerAction(page, reverseAction, reverseSelector);

    // Collect samples.
    return await collectAnimationSamples(page, timeoutMs);
  } finally {
    await page.close();
  }
}

async function captureSwitchAnimation(
  browser: Browser,
  url: string,
  blazor: boolean,
  openSteps: InteractionStep[],
  animStep: InteractionStep,
  timeoutMs: number
): Promise<{ closing: import("../types.js").AnimationSample[]; opening: import("../types.js").AnimationSample[] }> {
  const page = await browser.newPage();

  try {
    await page.goto(url, { waitUntil: "networkidle" });

    if (blazor) {
      await page.waitForFunction('typeof window.Blazor !== "undefined"', null, {
        timeout: 15_000,
      });
      await page.waitForTimeout(500);
    }

    // Open item 1.
    await runInteractions(page, openSteps);

    // Install dual observer: panel 1 = closing, panel 2 = opening.
    await installDualAnimationObserver(
      page,
      animStep.targetSelector!,
      animStep.switchTargetSelector!
    );

    // Click item 2 to trigger the switch.
    await triggerAction(page, animStep.action, animStep.switchSelector!);

    // Collect both traces.
    return await collectDualAnimationSamples(page, timeoutMs);
  } finally {
    await page.close();
  }
}

async function triggerAction(
  page: Page,
  action: "click" | "rightclick" | "hover",
  selector: string
): Promise<void> {
  const locator = page.locator(selector).first();
  switch (action) {
    case "click":
      await locator.click();
      break;
    case "rightclick":
      await locator.click({ button: "right" });
      break;
    case "hover":
      await locator.hover();
      break;
  }
}

function outputCloseAnimationSummary(
  component: string,
  ref: AnimationTrace,
  test: AnimationTrace,
  status: "match" | "diff"
): void {
  const refClass = classifyAnimation(ref);
  const testClass = classifyAnimation(test);

  console.log(`ANIMATE /${component} (close)`);

  const refHeights = ref.samples.map((s) => s.height);
  const testHeights = test.samples.map((s) => s.height);

  const refStart = refHeights[0] ?? 0;
  const refEnd = refHeights[refHeights.length - 1] ?? 0;
  const testStart = testHeights[0] ?? 0;
  const testEnd = testHeights[testHeights.length - 1] ?? 0;

  console.log(
    `  ref:  ${ref.durationMs}ms, ${ref.samples.length} frames, ${refStart}px → ${refEnd}px (${refClass})`
  );
  console.log(
    `  test: ${test.durationMs}ms, ${test.samples.length} frames, ${testStart}px → ${testEnd}px (${testClass})`
  );

  const endState = compareEndState(ref, test);
  const label = status === "match" ? "PASS" : "FAIL";
  if (status === "diff") {
    if (!endState.match) {
      console.log(`  RESULT: ${label} — end state differs (ref: ${Math.round(endState.refEndRatio * 100)}%, test: ${Math.round(endState.testEndRatio * 100)}% of start height)`);
    } else if (testClass === "instant" && refClass === "smooth") {
      console.log(`  RESULT: ${label} — test has no animation`);
    } else if (testClass === "smooth" && refClass === "instant") {
      console.log(`  RESULT: ${label} — test has animation but ref does not`);
    } else {
      console.log(`  RESULT: ${label} — animation behaviour differs`);
    }
  } else {
    console.log(`  RESULT: ${label}`);
  }
}

function formatTrace(label: string, trace: AnimationTrace): string {
  const heights = trace.samples.map((s) => s.height);
  const start = heights[0] ?? 0;
  const end = heights[heights.length - 1] ?? 0;
  const cls = classifyAnimation(trace);
  return `    ${label}: ${trace.durationMs}ms, ${trace.samples.length} frames, ${start}px → ${end}px (${cls})`;
}

function outputSwitchAnimationSummary(
  component: string,
  report: SwitchAnimationReport
): void {
  console.log(`ANIMATE /${component} (switch)`);

  console.log("  closing panel:");
  console.log(formatTrace("ref ", report.closing.ref));
  console.log(formatTrace("test", report.closing.test));

  console.log("  opening panel:");
  console.log(formatTrace("ref ", report.opening.ref));
  console.log(formatTrace("test", report.opening.test));

  const closeMatch = classifyAnimation(report.closing.ref) === classifyAnimation(report.closing.test);
  const openMatch = classifyAnimation(report.opening.ref) === classifyAnimation(report.opening.test);

  if (report.status === "match") {
    console.log("  RESULT: PASS");
  } else {
    const failures: string[] = [];
    if (!closeMatch) {
      const testCls = classifyAnimation(report.closing.test);
      failures.push(`closing panel: test ${testCls === "instant" ? "has no animation" : "animation differs"}`);
    }
    if (!openMatch) {
      const testCls = classifyAnimation(report.opening.test);
      failures.push(`opening panel: test ${testCls === "instant" ? "has no animation" : "animation differs"}`);
    }
    console.log(`  RESULT: FAIL — ${failures.join("; ")}`);
  }
}
