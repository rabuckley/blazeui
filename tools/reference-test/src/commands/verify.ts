import { chromium } from "playwright";
import { extractStyles, extractFromPage } from "../extract/extractor.js";
import { diffSnapshots } from "../diff/differ.js";
import { interactionRegistry } from "../interactions/registry.js";
import { runInteractions } from "../interactions/runner.js";
import { runCloseMode, runSwitchMode } from "./animate.js";
import { detectOpacityFlash, checkOpenPosition, installAnimationObserver, collectAnimationSamples } from "../extract/animator.js";
import { analyzeDataAttributeWarnings } from "../diff/behavioral.js";
import { analyzeLabelForWiring } from "../diff/label-wiring.js";
import { isolationRegistry } from "../interactions/registry.js";
import { runIsolationTest } from "./isolate.js";
import { runReopenCheck } from "./reopen.js";
import type { DiffReport, VerifyResult } from "../types.js";
import type { PseudoState } from "../extract/pseudo-states.js";

/** Overlay components eligible for reopen resilience checks. */
const OVERLAY_SET = new Set([
  "context-menu", "popover", "menu", "select", "tooltip",
  "dialog", "alert-dialog", "drawer", "sheet",
]);

const ALL_COMPONENTS = [
  "button",
  "input",
  "checkbox",
  "switch",
  "toggle",
  "separator",
  "progress",
  "avatar",
  "slider",
  "radio",
  "toggle-group",
  "accordion",
  "tabs",
  "dialog",
  "alert-dialog",
  "drawer",
  "sheet",
  "select",
  "menu",
  "context-menu",
  "popover",
  "tooltip",
  "scroll-area",
  "sidebar",
  "collapsible",
  "combobox",
  "field",
  "preview-card",
  "toast",
  "menubar",
  "navigation-menu",
  "blocks/login-02",
  "blocks/sidebar-01",
  "blocks/sidebar-07",
];

interface VerifyCommandOptions {
  refBase?: string;
  testBase?: string;
  format?: "summary" | "json" | "detail";
  timeout?: string;
  resolved?: boolean;
  pseudoStates?: string;
}

export async function verifyCommand(
  components: string[],
  options: VerifyCommandOptions
): Promise<void> {
  const refBase = (options.refBase ?? "http://localhost:5200").replace(/\/$/, "");
  const testBase = (options.testBase ?? "http://127.0.0.1:5199").replace(/\/$/, "");
  const format = options.format ?? "summary";
  const timeoutMs = parseInt(options.timeout ?? "500", 10);
  const pseudoStates = options.pseudoStates
    ?.split(",")
    .map((s) => s.trim())
    .filter(Boolean) as PseudoState[] | undefined;
  const comps = components.length > 0 ? components : ALL_COMPONENTS;

  const browser = await chromium.launch({ headless: true });
  const results: VerifyResult[] = [];

  try {
    for (const comp of comps) {
      try {
        const result = await verifyComponent(
          browser, comp, refBase, testBase, timeoutMs, options.resolved, pseudoStates
        );
        results.push(result);
      } catch (err) {
        results.push({
          component: comp,
          css: { status: "error", diffCount: 0 },
          overall: "error",
        });
        if (format !== "json") {
          console.error(`ERROR /${comp} — ${err}`);
        }
      }
    }
  } finally {
    await browser.close();
  }

  // Output results.
  if (format === "json") {
    console.log(JSON.stringify(results, null, 2));
  } else {
    for (const r of results) {
      outputVerifySummary(r, format === "detail");
    }
  }

  // Exit code: 2 if any errors, 1 if any fails/warns, 0 if all pass.
  const hasErrors = results.some((r) => r.overall === "error");
  const hasFails = results.some((r) => r.overall === "fail" || r.overall === "warn");
  if (hasErrors) process.exit(2);
  if (hasFails) process.exit(1);
  process.exit(0);
}

async function verifyComponent(
  browser: import("playwright").Browser,
  comp: string,
  refBase: string,
  testBase: string,
  timeoutMs: number,
  resolved?: boolean,
  pseudoStates?: PseudoState[]
): Promise<VerifyResult> {
  // 1. CSS diff (includes pseudo-state diffs when requested).
  const cssResult = await runCssDiff(browser, comp, refBase, testBase, resolved, pseudoStates);

  // 2. Animation checks.
  const steps = interactionRegistry[comp];
  const animStep = steps?.find((s) => s.targetSelector);

  let animStatus: "match" | "diff" | "error" | "skipped" = "skipped";
  let switchStatus: "match" | "diff" | "error" | "skipped" = "skipped";
  let reboundStatus: "pass" | "fail" | "error" | "skipped" = "skipped";
  const animWarnings: string[] = [];

  if (animStep && steps) {
    try {
      const closeReport = await runCloseMode(
        browser, comp, refBase, testBase, steps, animStep, timeoutMs
      );
      animStatus = closeReport.status;

      // Close-animation rebound detection (test trace only).
      if (closeReport.rebound) {
        reboundStatus = closeReport.rebound.rebounded ? "fail" : "pass";
      }

      // Opacity flash detection — opacity reaches ~0 then snaps back to 1.0.
      const opacityFlash = detectOpacityFlash(closeReport.test.samples);
      if (opacityFlash.rebounded) {
        reboundStatus = "fail";
        animWarnings.push(
          `close opacity flash: opacity snapped back to ~1.0 at ${opacityFlash.reboundAtMs}ms after reaching ~0`
        );
      }
    } catch {
      animStatus = "error";
    }

    if (animStep.switchSelector && animStep.switchTargetSelector) {
      try {
        const switchReport = await runSwitchMode(
          browser, comp, refBase, testBase, steps, animStep, timeoutMs
        );
        switchStatus = switchReport.status;
      } catch {
        switchStatus = "error";
      }
    }
  }

  // 2b. Open-animation position check (BlazeUI only).
  // Verifies the first visible frame of the open animation starts near the
  // anchor, not at (0,0). Skipped for centered overlays (dialog, alert-dialog,
  // drawer, sheet) where the popup opens at viewport center, not near the trigger.
  let openPositionStatus: "pass" | "fail" | "error" | "skipped" = "skipped";
  if (animStep && steps && !animStep.skipOpenPos) {
    try {
      openPositionStatus = await runOpenPositionCheck(
        browser, comp, testBase, steps, animStep, timeoutMs
      );
    } catch {
      openPositionStatus = "error";
    }
  }

  // 3. Behavioral warnings.
  const warnings = [...cssResult.warnings, ...animWarnings];

  // 4. Re-open resilience check for overlay components.
  let reopenStatus: "pass" | "fail" | "error" | "skipped" = "skipped";
  const isOverlay = OVERLAY_SET.has(comp);
  const hasReverseSelector = animStep?.reverseSelector !== undefined;
  if (steps && animStep && (isOverlay || hasReverseSelector)) {
    try {
      const reopenResult = await runReopenCheck(
        browser, comp, testBase, steps, animStep, timeoutMs
      );
      reopenStatus = reopenResult.status;
    } catch {
      reopenStatus = "error";
    }
  }

  // 5. Multi-instance isolation test (if configured for this component).
  let isolationStatus: "pass" | "fail" | "error" | "skipped" = "skipped";
  const isoConfig = isolationRegistry[comp];
  if (isoConfig) {
    try {
      const isoResult = await runIsolationTest(browser, comp, testBase, isoConfig);
      isolationStatus = isoResult.status;
    } catch {
      isolationStatus = "error";
    }
  }

  // 6. Pseudo-state diff result.
  const psDiffCount = cssResult.report.pseudoStateDiffs?.length ?? 0;
  const psStatus: "match" | "diff" | "skipped" = pseudoStates
    ? (psDiffCount > 0 ? "diff" : "match")
    : "skipped";

  // 7. Aggregate overall status.
  const overall = computeOverall(
    cssResult.report.status, animStatus, switchStatus, warnings,
    isolationStatus, reboundStatus, reopenStatus, psStatus, openPositionStatus
  );

  return {
    component: comp,
    css: { status: cssResult.report.status, diffCount: cssResult.report.summary.withDiffs },
    animation: { status: animStatus, mode: "close" },
    switchAnimation: { status: switchStatus },
    closeRebound: { status: reboundStatus },
    openPosition: { status: openPositionStatus },
    reopen: { status: reopenStatus },
    behavioral: { warnings },
    isolation: { status: isolationStatus },
    pseudoStates: { status: psStatus, diffCount: psDiffCount },
    overall,
  };
}

async function runCssDiff(
  browser: import("playwright").Browser,
  comp: string,
  refBase: string,
  testBase: string,
  resolved?: boolean,
  pseudoStates?: PseudoState[]
): Promise<{ report: DiffReport; warnings: string[] }> {
  // Extract from reference app.
  const refResult = await extractStyles({
    url: `${refBase}/${comp}`,
    browser,
    pseudoStates,
  });

  const steps = interactionRegistry[comp];
  if (steps) {
    await runInteractions(refResult.page, steps);
    refResult.snapshot.elements = await extractFromPage(refResult.page);
  }
  await refResult.page.close();

  // Extract from test app (Blazor).
  const testResult = await extractStyles({
    url: `${testBase}/${comp}`,
    blazor: true,
    browser,
    pseudoStates,
  });

  if (steps) {
    await runInteractions(testResult.page, steps);
    testResult.snapshot.elements = await extractFromPage(testResult.page);
  }
  await testResult.page.close();

  // Diff.
  const report = diffSnapshots(refResult.snapshot, testResult.snapshot, comp, { resolved });

  // Behavioral warnings.
  const dataWarnings = analyzeDataAttributeWarnings(
    refResult.snapshot.elements,
    testResult.snapshot.elements
  );
  const labelWarnings = analyzeLabelForWiring(
    refResult.snapshot.elements,
    testResult.snapshot.elements
  );
  const warnings = [...dataWarnings, ...labelWarnings];

  return { report, warnings };
}

function computeOverall(
  cssStatus: "match" | "diff" | "error",
  animStatus: "match" | "diff" | "error" | "skipped",
  switchStatus: "match" | "diff" | "error" | "skipped",
  warnings: string[],
  isolationStatus: "pass" | "fail" | "error" | "skipped" = "skipped",
  reboundStatus: "pass" | "fail" | "error" | "skipped" = "skipped",
  reopenStatus: "pass" | "fail" | "error" | "skipped" = "skipped",
  pseudoStatus: "match" | "diff" | "error" | "skipped" = "skipped",
  openPositionStatus: "pass" | "fail" | "error" | "skipped" = "skipped"
): "pass" | "warn" | "fail" | "error" {
  if (
    cssStatus === "error" ||
    animStatus === "error" ||
    switchStatus === "error" ||
    isolationStatus === "error" ||
    reboundStatus === "error" ||
    reopenStatus === "error" ||
    pseudoStatus === "error" ||
    openPositionStatus === "error"
  ) {
    return "error";
  }
  if (
    cssStatus === "diff" ||
    animStatus === "diff" ||
    switchStatus === "diff" ||
    isolationStatus === "fail" ||
    reboundStatus === "fail" ||
    reopenStatus === "fail" ||
    pseudoStatus === "diff" ||
    openPositionStatus === "fail"
  ) {
    return "fail";
  }
  if (warnings.length > 0) {
    return "warn";
  }
  return "pass";
}

function outputVerifySummary(result: VerifyResult, detail: boolean): void {
  console.log(`VERIFY /${result.component}`);

  // CSS status.
  const cssLabel = result.css.status === "match"
    ? "MATCH"
    : result.css.status === "error"
      ? "ERROR"
      : `DIFF (${result.css.diffCount} elements)`;
  console.log(`  css:       ${cssLabel}`);

  // Animation status.
  if (result.animation) {
    const animLabel = formatAnimStatus(result.animation.status);
    console.log(`  animate:   ${animLabel}`);
  }

  // Switch animation status.
  if (result.switchAnimation && result.switchAnimation.status !== "skipped") {
    const switchLabel = formatAnimStatus(result.switchAnimation.status);
    console.log(`  switch:    ${switchLabel}`);
  }

  // Close-animation rebound / opacity flash.
  if (result.closeRebound) {
    const reboundLabel = formatCheckStatus(result.closeRebound.status);
    console.log(`  rebound:   ${reboundLabel}`);
  }

  // Open-animation position check.
  if (result.openPosition && result.openPosition.status !== "skipped") {
    const openPosLabel = formatCheckStatus(result.openPosition.status);
    console.log(`  open-pos:  ${openPosLabel}`);
  }

  // Re-open resilience.
  if (result.reopen) {
    const reopenLabel = formatCheckStatus(result.reopen.status);
    console.log(`  reopen:    ${reopenLabel}`);
  }

  // Behavioral warnings.
  if (result.behavioral) {
    const warnCount = result.behavioral.warnings.length;
    if (warnCount > 0) {
      console.log(`  behavior:  ${warnCount} warning${warnCount > 1 ? "s" : ""}`);
      if (detail) {
        for (const w of result.behavioral.warnings) {
          console.log(`    ${w}`);
        }
      }
    } else {
      console.log("  behavior:  ok");
    }
  }

  // Pseudo-state status.
  if (result.pseudoStates && result.pseudoStates.status !== "skipped") {
    const psLabel = result.pseudoStates.status === "match"
      ? "MATCH"
      : result.pseudoStates.status === "error"
        ? "ERROR"
        : `DIFF (${result.pseudoStates.diffCount} elements)`;
    console.log(`  pseudo:    ${psLabel}`);
  }

  // Isolation status.
  if (result.isolation) {
    const isoLabel = result.isolation.status === "pass"
      ? "PASS"
      : result.isolation.status === "fail"
        ? "FAIL"
        : result.isolation.status === "error"
          ? "ERROR"
          : "skipped (no isolation config)";
    console.log(`  isolation: ${isoLabel}`);
  }

  // Overall.
  const overallLabel = result.overall.toUpperCase();
  console.log(`  OVERALL:   ${overallLabel}`);
  console.log();
}

function formatAnimStatus(status: "match" | "diff" | "error" | "skipped"): string {
  switch (status) {
    case "match": return "PASS";
    case "diff": return "FAIL";
    case "error": return "ERROR";
    case "skipped": return "skipped (no targetSelector)";
  }
}

function formatCheckStatus(status: "pass" | "fail" | "error" | "skipped"): string {
  switch (status) {
    case "pass": return "PASS";
    case "fail": return "FAIL";
    case "error": return "ERROR";
    case "skipped": return "skipped";
  }
}

/**
 * Opens a component on the BlazeUI app and checks that the first visible
 * frame of the popup animation appears near the anchor element, not at (0,0).
 * Catches the slide-from-origin bug where Blazor sets data-open before JS
 * positions the popup.
 */
async function runOpenPositionCheck(
  browser: import("playwright").Browser,
  comp: string,
  testBase: string,
  steps: import("../types.js").InteractionStep[],
  animStep: import("../types.js").InteractionStep,
  timeoutMs: number
): Promise<"pass" | "fail" | "error"> {
  const page = await browser.newPage();
  try {
    await page.goto(`${testBase}/${comp}`, { waitUntil: "networkidle" });
    await page.waitForFunction(
      'typeof window.Blazor !== "undefined"',
      null,
      { timeout: 15_000 }
    );
    await page.waitForTimeout(500);

    // Get the anchor element's position (the trigger or input).
    const anchorSel = animStep.selector;
    const anchorRect = await page.locator(anchorSel).first().boundingBox();
    if (!anchorRect) return "error";

    // Install observer before opening.
    await installAnimationObserver(page, animStep.targetSelector!);

    // Open the component.
    await runInteractions(page, steps);

    // Collect samples.
    const samples = await collectAnimationSamples(page, Math.min(timeoutMs, 300));

    // For context menus (rightclick), the popup opens at the cursor which
    // Playwright places at the center of the trigger, not at its bottom edge.
    const isRightClick = animStep.action === "rightclick";
    const adjustedRect = isRightClick
      ? {
          x: anchorRect.x + anchorRect.width / 2,
          y: anchorRect.y + anchorRect.height / 2,
          width: 0,
          height: 0,
        }
      : anchorRect;

    // Check the first visible frame's position.
    const result = checkOpenPosition(samples, adjustedRect);
    return result.ok ? "pass" : "fail";
  } finally {
    await page.close();
  }
}
