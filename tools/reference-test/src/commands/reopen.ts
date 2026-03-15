import type { Browser, Page } from "playwright";
import type { InteractionStep, ReopenResult } from "../types.js";
import { runInteractions } from "../interactions/runner.js";

/**
 * Tests close→open race condition resilience. Opens a component, closes it,
 * immediately re-opens, then verifies the target element is visible and
 * positionally stable. This catches Blazor Server async render pipeline
 * races where _pendingAction close and open collide.
 */
export async function runReopenCheck(
  browser: Browser,
  comp: string,
  testBase: string,
  steps: InteractionStep[],
  animStep: InteractionStep,
  timeoutMs: number
): Promise<ReopenResult> {
  const page = await browser.newPage();

  try {
    await page.goto(`${testBase}/${comp}`, { waitUntil: "networkidle" });
    await page.waitForFunction('typeof window.Blazor !== "undefined"', null, {
      timeout: 15_000,
    });
    await page.waitForTimeout(500);

    // Determine the target element to check after re-open.
    const targetSelector = animStep.targetSelector
      ?? inferTargetSelector(comp);
    if (!targetSelector) {
      return { status: "skipped", details: "no target selector" };
    }

    // Determine close action.
    const reverseAction = animStep.reverseAction ?? animStep.action;
    const reverseSelector = animStep.reverseSelector ?? animStep.selector;

    // 1. Open the component.
    await runInteractions(page, steps);

    // 2. Close via reverse action.
    await triggerAction(page, reverseAction, reverseSelector);

    // 3. Wait only 50ms — we want to test the race, not wait for animation.
    await page.waitForTimeout(50);

    // 4. Re-open immediately.
    await runInteractions(page, steps);

    // 5. Wait 100ms for render to settle.
    await page.waitForTimeout(100);

    // 6. Check target element visibility.
    const locator = page.locator(targetSelector).first();
    const visible = await locator.isVisible().catch(() => false);

    if (!visible) {
      return {
        status: "fail",
        visible: false,
        positionStable: false,
        details: `target "${targetSelector}" not visible after re-open`,
      };
    }

    // 7. Sample position at 50ms intervals for 200ms — check stability.
    const positions: { x: number; y: number }[] = [];
    for (let i = 0; i < 4; i++) {
      const box = await locator.boundingBox();
      if (box) {
        positions.push({ x: box.x, y: box.y });
      }
      if (i < 3) await page.waitForTimeout(50);
    }

    // Position is stable if all samples are within 2px of each other.
    let positionStable = true;
    if (positions.length >= 2) {
      const first = positions[0]!;
      for (let i = 1; i < positions.length; i++) {
        const pos = positions[i]!;
        if (Math.abs(pos.x - first.x) > 2 || Math.abs(pos.y - first.y) > 2) {
          positionStable = false;
          break;
        }
      }
    }

    if (!positionStable) {
      return {
        status: "fail",
        visible: true,
        positionStable: false,
        details: "position unstable after re-open (sliding/jumping)",
      };
    }

    return { status: "pass", visible: true, positionStable: true };
  } catch (err) {
    return {
      status: "error",
      details: `reopen check failed: ${err}`,
    };
  } finally {
    await page.close();
  }
}

/**
 * Infer a target selector for overlay components that don't have an
 * explicit targetSelector in their interaction step.
 */
function inferTargetSelector(comp: string): string | undefined {
  const heuristics: Record<string, string> = {
    "context-menu": "[role=menu]",
    menu: "[role=menu]",
    popover: "[data-slot=popover-content]",
    select: "[role=listbox]",
    tooltip: "[role=tooltip]",
    dialog: "[role=dialog]",
    "alert-dialog": "[role=alertdialog]",
    drawer: "[role=dialog]",
  };
  return heuristics[comp];
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
