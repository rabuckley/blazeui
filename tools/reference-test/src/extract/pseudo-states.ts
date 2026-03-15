import type { Page } from "playwright";
import type { ElementSnapshot, PseudoStateDelta, PseudoStateResult } from "../types.js";

export type PseudoState =
  | "hover"
  | "focus"
  | "focus-visible"
  | "active"
  | "focus-within";

/**
 * CSS selector covering elements that respond to interactive pseudo-states.
 * Structural elements (divs, spans) without explicit interactivity are
 * excluded to keep extraction fast.
 */
const INTERACTIVE_SELECTOR = [
  "button", "a", "input", "select", "textarea",
  "[tabindex]", "[role='button']", "[role='link']", "[role='tab']",
  "[role='menuitem']", "[role='menuitemcheckbox']", "[role='menuitemradio']",
  "[role='option']", "[role='switch']", "[role='checkbox']",
  "[role='radio']", "[role='slider']", "[role='combobox']",
].join(", ");

/**
 * Activates pseudo-states on interactive elements via real Playwright
 * interactions, captures computed style deltas, then resets.
 *
 * CDP's `CSS.forcePseudoState` doesn't trigger styles guarded by
 * `@media (hover: hover)`, which Tailwind v4 emits for all hover
 * variants. Real mouse hover via Playwright does, so we use that
 * approach instead.
 *
 * For each interactive element, the algorithm:
 * 1. Tags it with `data-__psi` for correlation with baseElements.
 * 2. Triggers the pseudo-state via Playwright (hover, focus, etc.).
 * 3. Reads computed styles for that single element.
 * 4. Resets the state (move mouse away, blur, etc.).
 * 5. Diffs against baseElements to produce a compact delta.
 */
export async function extractPseudoStateDeltas(
  page: Page,
  baseElements: ElementSnapshot[],
  states: PseudoState[],
  scope?: string
): Promise<PseudoStateResult[]> {
  // Tag visible elements with data-__psi to correlate with baseElements
  // indices. Returns the indices of interactive elements.
  const interactiveIndices: number[] = await page.evaluate(
    ({ scopeArg, interactiveSel }: {
      scopeArg: string | undefined;
      interactiveSel: string;
    }) => {
      const root = scopeArg ? document.querySelector(scopeArg) : document;
      if (!root) return [];

      const allEls = Array.from(root.querySelectorAll("*")).filter((el) => {
        const cs = getComputedStyle(el);
        if (cs.display === "none" || cs.visibility === "hidden") return false;
        const rect = el.getBoundingClientRect();
        return rect.width > 0 || rect.height > 0;
      });

      const interactive: number[] = [];
      for (let i = 0; i < allEls.length; i++) {
        const el = allEls[i]!;
        (el as HTMLElement).setAttribute("data-__psi", String(i));
        if (el.matches(interactiveSel)) {
          interactive.push(i);
        }
      }
      return interactive;
    },
    { scopeArg: scope, interactiveSel: INTERACTIVE_SELECTOR }
  );

  const results: PseudoStateResult[] = [];

  try {
    for (const state of states) {
      const deltas: PseudoStateDelta[] = [];

      for (const idx of interactiveIndices) {
        if (idx >= baseElements.length) continue;

        const base = baseElements[idx]!;
        const selector = `[data-__psi='${idx}']`;
        const locator = page.locator(selector);

        // Verify element is still in DOM and visible.
        if ((await locator.count()) === 0) continue;

        try {
          // Trigger the pseudo-state.
          await triggerState(page, locator, state);

          // Capture computed styles for this single element.
          const forcedProps = await locator.evaluate((el) => {
            const cs = getComputedStyle(el);
            const props: Record<string, string> = {};
            for (let i = 0; i < cs.length; i++) {
              const name = cs[i]!;
              props[name] = cs.getPropertyValue(name);
            }
            const rect = el.getBoundingClientRect();
            props["_h"] = rect.height + "px";
            props["_w"] = rect.width + "px";
            return props;
          });

          // Reset the state before moving to the next element.
          await resetState(page, locator, state);

          // Compute delta: only properties that changed from base.
          const changedProps: Record<string, string> = {};
          for (const [prop, forcedVal] of Object.entries(forcedProps)) {
            const baseVal = base.props[prop] ?? "";
            if (forcedVal !== baseVal) {
              changedProps[prop] = forcedVal;
            }
          }

          if (Object.keys(changedProps).length > 0) {
            deltas.push({
              elementIndex: idx,
              key: base.key,
              tag: base.tag,
              changedProps,
            });
          }
        } catch {
          // Element may have become detached or non-interactive; skip.
        }
      }

      results.push({ state, deltas });
    }
  } finally {
    // Clean up marker attributes.
    await page.evaluate((scopeArg: string | undefined) => {
      const root = scopeArg ? document.querySelector(scopeArg) : document;
      if (!root) return;
      for (const el of Array.from(root.querySelectorAll("[data-__psi]"))) {
        el.removeAttribute("data-__psi");
      }
    }, scope);
  }

  return results;
}

/**
 * Triggers a pseudo-state on an element using real Playwright interactions.
 * This approach works with Tailwind v4's `@media (hover: hover)` guards
 * that CDP pseudo-state forcing does not activate.
 */
async function triggerState(
  page: Page,
  locator: import("playwright").Locator,
  state: PseudoState
): Promise<void> {
  switch (state) {
    case "hover":
      await locator.hover({ timeout: 2000 });
      break;
    case "focus":
      await locator.focus({ timeout: 2000 });
      break;
    case "focus-visible":
      // Tab to the element to trigger :focus-visible (keyboard focus).
      await locator.focus({ timeout: 2000 });
      await page.keyboard.press("Tab");
      await page.keyboard.press("Shift+Tab");
      // Fallback: just focus — some browsers treat programmatic focus
      // as focus-visible when the element is keyboard-focusable.
      await locator.focus({ timeout: 2000 });
      break;
    case "active":
      // Mouse down without releasing triggers :active.
      await locator.hover({ timeout: 2000 });
      await page.mouse.down();
      break;
    case "focus-within":
      await locator.focus({ timeout: 2000 });
      break;
  }

  // Brief settle time for style recalculation.
  await page.waitForTimeout(50);
}

/**
 * Resets a pseudo-state triggered by triggerState().
 */
async function resetState(
  page: Page,
  _locator: import("playwright").Locator,
  state: PseudoState
): Promise<void> {
  switch (state) {
    case "hover":
      // Move mouse to a neutral position off-screen.
      await page.mouse.move(0, 0);
      break;
    case "focus":
    case "focus-visible":
    case "focus-within":
      // Blur by clicking on body.
      await page.evaluate(() => {
        (document.activeElement as HTMLElement)?.blur?.();
      });
      break;
    case "active":
      await page.mouse.up();
      await page.mouse.move(0, 0);
      break;
  }
}
