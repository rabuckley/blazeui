import type { Page } from "playwright";
import type { InteractionStep } from "../types.js";

const SETTLE_DELAY_MS = 300;

/**
 * Executes a sequence of interaction steps on a Playwright page,
 * then waits for animations to settle.
 */
export async function runInteractions(
  page: Page,
  steps: InteractionStep[]
): Promise<void> {
  for (const step of steps) {
    const locator = step.nth != null
      ? page.locator(step.selector).nth(step.nth)
      : page.locator(step.selector).first();

    switch (step.action) {
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

  // Let animations and transitions settle.
  await page.waitForTimeout(SETTLE_DELAY_MS);
}
