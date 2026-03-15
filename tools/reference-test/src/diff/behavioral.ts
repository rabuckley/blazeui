import type { ElementSnapshot } from "../types.js";

/**
 * Analyzes test elements for Tailwind data-attribute variant classes that
 * reference attributes not actually present on the relevant element.
 *
 * Tailwind generates classes like `data-[state=open]:rotate-180` which compile
 * to `[data-state="open"]` CSS selectors. If the element doesn't have
 * `data-state`, the class never activates — a silent behavioral bug that
 * won't appear in a CSS property diff (since the property just keeps its
 * default value).
 *
 * Contextual selectors reference other elements in the DOM tree:
 * - `group-data-[attr]` → nearest ancestor with a `group` class
 * - `peer-data-[attr]`  → preceding sibling with a `peer` class
 * - `has-data-[attr]`   → any descendant of this element
 * - `in-data-[attr]`    → any ancestor of this element
 *
 * The checker resolves these using the parentIndex tree captured during
 * extraction, so it checks the right element for each selector type.
 */
export function analyzeDataAttributeWarnings(
  refElements: ElementSnapshot[],
  testElements: ElementSnapshot[]
): string[] {
  const warnings: string[] = [];

  const len = Math.min(refElements.length, testElements.length);

  // Pre-build descendant lookup for has-data- checks: for each element
  // index, which indices are descendants?
  const testDescendants = buildDescendantMap(testElements);
  const refDescendants = buildDescendantMap(refElements);

  for (let i = 0; i < len; i++) {
    const testEl = testElements[i]!;
    const refEl = refElements[i]!;
    if (!testEl.className) continue;

    // Extract data-[...] variant patterns from className.
    // Captures: optional contextual prefix and attribute name.
    const dataVariantPattern =
      /((?:group|peer|has|in)-)?data-\[([^\]=]+)(?:=[^\]]+)?\]/g;
    let match;

    while ((match = dataVariantPattern.exec(testEl.className)) !== null) {
      let prefix = match[1] as string | undefined; // e.g. "group-", "peer-", "has-", "in-", or undefined
      const fullMatch = match[0]!;
      const attrName = match[2]!;

      const classToken = findClassToken(testEl.className, fullMatch);

      // The regex captures "group-" when it's immediately before data-[...],
      // but stacked variants like "md:peer-data-[...]" have the prefix
      // before a colon. Check the full class token for those cases.
      if (!prefix && isContextualSelector(classToken, fullMatch)) {
        prefix = extractContextualPrefix(classToken, fullMatch);
      }

      const found = resolveAttribute(
        prefix, attrName, i, testElements, testDescendants
      );
      const refFound = resolveAttribute(
        prefix, attrName, i, refElements, refDescendants
      );

      // Only warn when the reference can resolve the attribute but the
      // test cannot. If neither resolves it, the attribute is
      // state-dependent and just not active in the current snapshot.
      if (!found && refFound) {
        const tagLabel = testEl.tag;
        const context = prefix ? ` (via ${prefix.slice(0, -1)} context)` : "";
        warnings.push(
          `[${i}] ${tagLabel} .${classToken} — data-${attrName} not present on test element${context}`
        );
      }
    }
  }

  return warnings;
}

/**
 * Resolves whether a data attribute is present on the appropriate element
 * given the Tailwind selector prefix.
 */
function resolveAttribute(
  prefix: string | undefined,
  attrName: string,
  elementIndex: number,
  elements: ElementSnapshot[],
  descendantMap: Map<number, number[]>
): boolean {
  const el = elements[elementIndex];
  if (!el) return false;

  if (!prefix) {
    // Direct: data-[attr] — check this element.
    return attrName in (el.dataAttributes ?? {});
  }

  const kind = prefix.replace(/-$/, ""); // "group", "peer", "has", "in"

  switch (kind) {
    case "group":
      return hasAttrOnAncestorWithClass(
        elementIndex, "group", attrName, elements
      );

    case "peer":
      return hasAttrOnPeerWithClass(
        elementIndex, "peer", attrName, elements
      );

    case "has":
      return hasAttrOnDescendant(
        elementIndex, attrName, elements, descendantMap
      );

    case "in":
      return hasAttrOnAnyAncestor(elementIndex, attrName, elements);

    default:
      return false;
  }
}

/**
 * Walks the parentIndex chain looking for an ancestor whose className
 * contains the given class word (e.g. "group"), then checks if that
 * ancestor has the data attribute.
 */
function hasAttrOnAncestorWithClass(
  startIndex: number,
  targetClass: string,
  attrName: string,
  elements: ElementSnapshot[]
): boolean {
  // The Tailwind group class can appear as "group" or "group/name".
  const pattern = new RegExp(`(?:^|\\s)${targetClass}(?:\\/|\\s|$)`);
  let idx = elements[startIndex]?.parentIndex ?? -1;

  while (idx >= 0 && idx < elements.length) {
    const ancestor = elements[idx]!;
    if (ancestor.className && pattern.test(ancestor.className)) {
      return attrName in (ancestor.dataAttributes ?? {});
    }
    idx = ancestor.parentIndex ?? -1;
  }

  return false;
}

/**
 * Looks for a preceding sibling with the given class (e.g. "peer") and
 * checks if it has the data attribute. Siblings share the same parentIndex.
 */
function hasAttrOnPeerWithClass(
  elementIndex: number,
  targetClass: string,
  attrName: string,
  elements: ElementSnapshot[]
): boolean {
  const el = elements[elementIndex]!;
  const parentIdx = el.parentIndex ?? -1;
  const pattern = new RegExp(`(?:^|\\s)${targetClass}(?:\\/|\\s|$)`);

  // Walk backwards through elements with the same parent, looking for a
  // peer. Preceding siblings appear earlier in the flat array.
  for (let j = elementIndex - 1; j >= 0; j--) {
    const candidate = elements[j]!;
    if ((candidate.parentIndex ?? -1) !== parentIdx) continue;
    if (candidate.className && pattern.test(candidate.className)) {
      return attrName in (candidate.dataAttributes ?? {});
    }
  }

  return false;
}

/**
 * Checks if any descendant of the element has the data attribute.
 */
function hasAttrOnDescendant(
  elementIndex: number,
  attrName: string,
  elements: ElementSnapshot[],
  descendantMap: Map<number, number[]>
): boolean {
  const descendants = descendantMap.get(elementIndex);
  if (!descendants) return false;

  for (const idx of descendants) {
    if (attrName in (elements[idx]!.dataAttributes ?? {})) {
      return true;
    }
  }
  return false;
}

/**
 * Walks the parentIndex chain checking if any ancestor has the data attribute.
 */
function hasAttrOnAnyAncestor(
  startIndex: number,
  attrName: string,
  elements: ElementSnapshot[]
): boolean {
  let idx = elements[startIndex]?.parentIndex ?? -1;

  while (idx >= 0 && idx < elements.length) {
    if (attrName in (elements[idx]!.dataAttributes ?? {})) {
      return true;
    }
    idx = elements[idx]!.parentIndex ?? -1;
  }

  return false;
}

/**
 * Builds a map from each element index to the list of all its descendant
 * indices, used for has-data-[...] checks.
 */
function buildDescendantMap(
  elements: ElementSnapshot[]
): Map<number, number[]> {
  const map = new Map<number, number[]>();

  for (let i = 0; i < elements.length; i++) {
    // Walk up the parent chain and register this element as a descendant
    // of every ancestor.
    let parentIdx = elements[i]!.parentIndex ?? -1;
    while (parentIdx >= 0) {
      let list = map.get(parentIdx);
      if (!list) {
        list = [];
        map.set(parentIdx, list);
      }
      list.push(i);
      parentIdx = elements[parentIdx]!.parentIndex ?? -1;
    }
  }

  return map;
}

/**
 * Determines whether a class token uses a contextual Tailwind selector
 * (group-data-, peer-data-, has-data-, in-data-) that references a
 * different element in the DOM tree via stacked variant syntax
 * (e.g. "md:peer-data-[variant=inset]:m-2").
 */
function isContextualSelector(
  classToken: string,
  variantPattern: string
): boolean {
  const idx = classToken.indexOf(variantPattern);
  if (idx <= 0) return false;

  const before = classToken.substring(0, idx);
  return /(?:^|:)(?:group|peer|has|in)-$/i.test(before);
}

/**
 * Extracts the contextual prefix from a stacked variant class token.
 * e.g. "md:peer-data-[variant=inset]:m-2" → "peer-"
 */
function extractContextualPrefix(
  classToken: string,
  variantPattern: string
): string | undefined {
  const idx = classToken.indexOf(variantPattern);
  if (idx <= 0) return undefined;

  const before = classToken.substring(0, idx);
  const m = before.match(/(?:^|:)((?:group|peer|has|in)-)$/i);
  return m ? m[1] : undefined;
}

/**
 * Finds the full space-delimited class token that contains the given
 * variant pattern.
 */
function findClassToken(className: string, variantPattern: string): string {
  const tokens = className.split(/\s+/);
  for (const token of tokens) {
    if (token.includes(variantPattern)) {
      return token;
    }
  }
  return variantPattern;
}
