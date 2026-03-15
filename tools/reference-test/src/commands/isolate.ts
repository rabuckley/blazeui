import { chromium, type Browser, type Page } from "playwright";
import { isolationRegistry } from "../interactions/registry.js";
import type { IsolationConfig, IsolationResult, IsolationDetail } from "../types.js";

interface IsolateCommandOptions {
  testBase?: string;
  format?: "summary" | "json";
}

export async function isolateCommand(
  component: string,
  options: IsolateCommandOptions
): Promise<void> {
  const testBase = (options.testBase ?? "http://127.0.0.1:5199").replace(/\/$/, "");
  const format = options.format ?? "summary";

  const config = isolationRegistry[component];
  if (!config) {
    console.error(
      `No isolation config for "${component}". ` +
        `Available: ${Object.keys(isolationRegistry).join(", ")}`
    );
    process.exit(2);
  }

  const browser = await chromium.launch({ headless: true });

  try {
    const result = await runIsolationTest(browser, component, testBase, config);

    if (format === "json") {
      console.log(JSON.stringify(result, null, 2));
    } else {
      outputIsolationSummary(result);
    }

    process.exit(result.status === "pass" ? 0 : result.status === "skipped" ? 0 : 1);
  } finally {
    await browser.close();
  }
}

/**
 * Tests that multiple instances of a component on the same page don't
 * share JS state. Interacts with each instance one at a time and checks
 * that only the targeted instance's state changes.
 */
export async function runIsolationTest(
  browser: Browser,
  component: string,
  testBase: string,
  config: IsolationConfig
): Promise<IsolationResult> {
  const url = `${testBase}/${component}`;
  const details: IsolationDetail[] = [];
  let instanceCount = 0;

  // Initial page load to count instances.
  const probePage = await browser.newPage();
  try {
    await probePage.goto(url, { waitUntil: "networkidle" });
    await probePage.waitForFunction('typeof window.Blazor !== "undefined"', null, {
      timeout: 15_000,
    });
    await probePage.waitForTimeout(500);

    const instances = probePage.locator(config.selector);
    instanceCount = await instances.count();
  } finally {
    await probePage.close();
  }

  if (instanceCount < 2) {
    return {
      component,
      status: "skipped",
      instanceCount,
      details: [],
    };
  }

  // Test each instance in isolation. Reload the page between interactions
  // to reset state, ensuring each test starts from a clean slate.
  for (let i = 0; i < instanceCount; i++) {
    const page = await browser.newPage();

    try {
      await page.goto(url, { waitUntil: "networkidle" });
      await page.waitForFunction('typeof window.Blazor !== "undefined"', null, {
        timeout: 15_000,
      });
      await page.waitForTimeout(500);

      const instances = page.locator(config.selector);

      // Read state from all instances before interaction.
      const beforeStates = await readAllStates(instances, instanceCount, config.stateAttr, config.stateSelector);

      // Interact with instance i.
      await interactWithInstance(page, instances, i, config);
      await page.waitForTimeout(300);

      // Read state from all instances after interaction.
      const afterStates = await readAllStates(instances, instanceCount, config.stateAttr, config.stateSelector);

      // Check: did the interacted instance change?
      const changed = beforeStates[i] !== afterStates[i];

      // Detect if the instance is disabled (aria-disabled or data-disabled).
      const instance = instances.nth(i);
      const isDisabled =
        (await instance.getAttribute("data-disabled")) !== null ||
        (await instance.getAttribute("aria-disabled")) === "true" ||
        (await instance.getAttribute("disabled")) !== null;

      // Check: did any OTHER instance change (leaked state)?
      const leakedIndices: number[] = [];
      for (let j = 0; j < instanceCount; j++) {
        if (j !== i && beforeStates[j] !== afterStates[j]) {
          leakedIndices.push(j);
        }
      }

      details.push({
        interactedIndex: i,
        changed,
        disabled: isDisabled,
        leakedIndices,
        states: { before: beforeStates, after: afterStates },
      });
    } finally {
      await page.close();
    }
  }

  const anyLeaks = details.some((d) => d.leakedIndices.length > 0);
  // Disabled instances are not expected to change when interacted with.
  const enabledDetails = details.filter((d) => !d.disabled);
  const allEnabledChanged = enabledDetails.length === 0 || enabledDetails.every((d) => d.changed);

  return {
    component,
    status: anyLeaks ? "fail" : allEnabledChanged ? "pass" : "fail",
    instanceCount,
    details,
  };
}

async function readAllStates(
  instances: import("playwright").Locator,
  count: number,
  attr: string,
  stateSelector?: string
): Promise<string[]> {
  const states: string[] = [];
  for (let i = 0; i < count; i++) {
    const target = stateSelector
      ? instances.nth(i).locator(stateSelector).first()
      : instances.nth(i);
    const value = await target.getAttribute(attr) ?? "";
    states.push(value);
  }
  return states;
}

async function interactWithInstance(
  page: Page,
  instances: import("playwright").Locator,
  index: number,
  config: IsolationConfig
): Promise<void> {
  const instance = instances.nth(index);

  if (config.action === "click") {
    await instance.click();
    return;
  }

  // Drag interaction: move the thumb by a fraction of the element's width.
  const box = await instance.boundingBox();
  if (!box) return;

  const centerX = box.x + box.width / 2;
  const centerY = box.y + box.height / 2;
  const offset = (config.dragOffset ?? 0.3) * box.width;

  await page.mouse.move(centerX, centerY);
  await page.mouse.down();
  await page.mouse.move(centerX + offset, centerY, { steps: 5 });
  await page.mouse.up();
}

function outputIsolationSummary(result: IsolationResult): void {
  const statusTag = result.status.toUpperCase();
  console.log(`${statusTag} /${result.component} — ${result.instanceCount} instances`);

  if (result.status === "skipped") {
    console.log("  Skipped: fewer than 2 instances found");
    return;
  }

  for (const d of result.details) {
    const changeLabel = d.disabled
      ? "disabled (skipped)"
      : d.changed
        ? "changed"
        : "DID NOT CHANGE";
    const leakLabel =
      d.leakedIndices.length > 0
        ? ` — LEAKED to instance${d.leakedIndices.length > 1 ? "s" : ""} ${d.leakedIndices.join(", ")}`
        : "";
    console.log(`  [${d.interactedIndex}] ${changeLabel}${leakLabel}`);
  }
}
