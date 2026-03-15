import type { Page } from "playwright";

/**
 * Evaluates a string expression in the browser context.
 *
 * All page.evaluate calls in this tool must use string expressions rather than
 * function callbacks because tsx/esbuild injects a __name helper for named
 * functions that doesn't exist in the browser context. This helper centralizes
 * that pattern so callers don't need to remember the workaround.
 */
export async function evaluateString<T>(
  page: Page,
  expression: string
): Promise<T> {
  return (await page.evaluate(expression)) as T;
}

/**
 * Escapes a CSS selector for safe interpolation into a string expression.
 * Handles single quotes which would break template string boundaries.
 */
export function escapeSelector(selector: string): string {
  return selector.replace(/'/g, "\\'");
}
