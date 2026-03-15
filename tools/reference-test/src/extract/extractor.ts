import { chromium, type Browser, type Page } from "playwright";
import type { ElementSnapshot, StyleSnapshot } from "../types.js";
import type { PseudoState } from "./pseudo-states.js";

export interface ExtractOptions {
  url: string;
  blazor?: boolean;
  scope?: string;
  /** Existing browser instance to reuse (avoids re-launching). */
  browser?: Browser;
  /** Pseudo-states to force via CDP and capture deltas for. */
  pseudoStates?: PseudoState[];
}

/**
 * Runs page.evaluate() to capture every computed CSS property on every
 * visible element plus ::before/::after pseudo-elements. This is the
 * single source of truth for the extraction logic — called both during
 * initial extraction and after interactions.
 */
export async function extractFromPage(
  page: Page,
  scope?: string
): Promise<ElementSnapshot[]> {
  const elements = await page.evaluate((scopeArg: string | undefined) => {
    const root = scopeArg ? document.querySelector(scopeArg) : document;
    if (!root) return [];

    const results: Array<{
      key: string;
      tag: string;
      props: Record<string, string>;
      parentIndex?: number;
      role?: string;
      dataSlot?: string;
      className?: string;
      dataAttributes?: Record<string, string>;
      htmlAttributes?: Record<string, string>;
    }> = [];

    // Semantic HTML attributes to capture for behavioral analysis.
    const SEMANTIC_ATTRS = ["id", "for", "type", "tabindex", "name",
      "aria-labelledby", "aria-describedby", "aria-controls", "aria-owns"];

    // Track which DOM elements map to which result array index,
    // so we can resolve parentIndex as a post-filtering reference.
    const elementIndexMap = new Map<Element, number>();

    for (const el of Array.from(root.querySelectorAll("*"))) {
      const cs = getComputedStyle(el);
      if (cs.display === "none" || cs.visibility === "hidden") continue;

      const rect = el.getBoundingClientRect();
      if (rect.width === 0 && rect.height === 0) continue;

      const directText = Array.from(el.childNodes)
        .filter((n) => n.nodeType === 3)
        .map((n) => n.textContent ?? "")
        .join("")
        .trim()
        .substring(0, 30);

      const slot = (el as HTMLElement).dataset?.sidebar ?? "";

      const key =
        directText || (slot ? "slot:" + slot : "") || el.tagName;

      const props: Record<string, string> = {};
      for (let i = 0; i < cs.length; i++) {
        const name = cs[i]!;
        props[name] = cs.getPropertyValue(name);
      }

      // Bounding rect dimensions (not in CSSStyleDeclaration).
      props["_h"] = rect.height + "px";
      props["_w"] = rect.width + "px";

      // Walk up to find nearest ancestor already in the result array.
      let parentIndex = -1;
      let ancestor = el.parentElement;
      while (ancestor) {
        const idx = elementIndexMap.get(ancestor);
        if (idx !== undefined) {
          parentIndex = idx;
          break;
        }
        ancestor = ancestor.parentElement;
      }

      const role = el.getAttribute("role") ?? "";
      const dataSlot = el.getAttribute("data-slot") ?? "";
      const className = typeof el.className === "string" ? el.className : "";

      // Capture all data-* attributes for behavioral analysis.
      const dataAttributes: Record<string, string> = {};
      for (const attr of Array.from(el.attributes)) {
        if (attr.name.startsWith("data-")) {
          dataAttributes[attr.name.substring(5)] = attr.value;
        }
      }

      // Capture semantic HTML attributes for behavioral analysis.
      const htmlAttributes: Record<string, string> = {};
      for (const attrName of SEMANTIC_ATTRS) {
        const val = el.getAttribute(attrName);
        if (val !== null) htmlAttributes[attrName] = val;
      }

      const resultIndex = results.length;
      elementIndexMap.set(el, resultIndex);

      results.push({ key, tag: el.tagName, props, parentIndex, role, dataSlot, className, dataAttributes, htmlAttributes });

      // Capture pseudo-element styles when they have visible effect.
      const pseudos = [
        "::before", "::after", "::marker",
        "::placeholder", "::file-selector-button",
        "::backdrop", "::selection",
        "::first-line", "::first-letter",
      ] as const;

      for (const pseudo of pseudos) {
        const ps = getComputedStyle(el, pseudo);
        if (ps.content === "none" && ps.display === "none") continue;
        if (
          ps.width === "auto" &&
          ps.height === "auto" &&
          ps.borderWidth === "0px" &&
          ps.content === "none"
        )
          continue;

        const hasBorder = ps.borderWidth !== "0px";
        const hasBg = ps.backgroundColor !== "rgba(0, 0, 0, 0)";
        const hasContent =
          ps.content !== "none" &&
          ps.content !== '""' &&
          ps.content !== "normal";
        if (!hasBorder && !hasBg && !hasContent) continue;

        const pProps: Record<string, string> = {};
        for (let i = 0; i < ps.length; i++) {
          const name = ps[i]!;
          pProps[name] = ps.getPropertyValue(name);
        }
        // Pseudo-elements belong to their host element.
        results.push({
          key: key + pseudo,
          tag: pseudo,
          props: pProps,
          parentIndex: resultIndex,
          role: "",
          dataSlot: "",
          className: "",
        });
      }
    }

    return results;
  }, scope);

  return elements as ElementSnapshot[];
}

/**
 * Extracts computed CSS styles from all visible elements on a page.
 *
 * Launches headless Chromium (or reuses a provided browser), navigates to
 * the URL, optionally waits for the Blazor runtime, then extracts styles.
 */
export async function extractStyles(
  options: ExtractOptions
): Promise<{ snapshot: StyleSnapshot; page: Page; browser: Browser }> {
  const ownBrowser = !options.browser;
  const browser =
    options.browser ?? (await chromium.launch({ headless: true }));
  const page = await browser.newPage();

  try {
    await page.goto(options.url, { waitUntil: "networkidle" });

    if (options.blazor) {
      await page.waitForFunction(
        'typeof window.Blazor !== "undefined"',
        null,
        { timeout: 15_000 }
      );
      // Wait for SSR → interactive handoff to settle.
      await page.waitForTimeout(500);
    }

    const elements = await extractFromPage(page, options.scope);

    // Derive component name from the URL path.
    const urlObj = new URL(options.url);
    const component =
      urlObj.pathname.replace(/^\//, "").replace(/\/$/, "") || "index";

    const snapshot: StyleSnapshot = {
      url: options.url,
      component,
      timestamp: new Date().toISOString(),
      elements,
    };

    // Capture pseudo-state deltas if requested.
    if (options.pseudoStates && options.pseudoStates.length > 0) {
      const { extractPseudoStateDeltas } = await import("./pseudo-states.js");
      const psResults = await extractPseudoStateDeltas(
        page, elements, options.pseudoStates, options.scope
      );
      const pseudoStates: StyleSnapshot["pseudoStates"] = {};
      for (const r of psResults) {
        if (r.deltas.length > 0) {
          pseudoStates[r.state] = r.deltas;
        }
      }
      if (Object.keys(pseudoStates).length > 0) {
        snapshot.pseudoStates = pseudoStates;
      }
    }

    return { snapshot, page, browser };
  } catch (err) {
    await page.close();
    if (ownBrowser) await browser.close();
    throw err;
  }
}
