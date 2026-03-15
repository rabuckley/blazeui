import type { ElementSnapshot } from "../types.js";

/**
 * Compares label-for wiring between reference and test snapshots.
 *
 * For every `<label for="X">` in the reference, checks that the test has:
 * 1. A label with the same `for` value
 * 2. A target element with `id="X"`
 * 3. A target of the same tag type (e.g. both targeting an <input>)
 *
 * This catches bugs where:
 * - A hidden form input is missing (label targets wrong element or nothing)
 * - The id is on the wrong element (e.g. visible span instead of hidden input)
 * - The label-input association is completely absent
 */
export function analyzeLabelForWiring(
  refElements: ElementSnapshot[],
  testElements: ElementSnapshot[]
): string[] {
  const warnings: string[] = [];

  const refIdMap = buildIdMap(refElements);
  const testIdMap = buildIdMap(testElements);

  // Find all labels with `for` in the reference.
  for (let i = 0; i < refElements.length; i++) {
    const refEl = refElements[i]!;
    if (refEl.tag !== "LABEL") continue;

    const forValue = refEl.htmlAttributes?.["for"];
    if (!forValue) continue;

    const refTarget = refIdMap.get(forValue);

    // Find a matching label in the test. Look for a label with the same
    // text content (key) and same `for` value.
    const testLabel = findMatchingLabel(refEl, testElements);

    if (!testLabel) {
      // Test has no label with this for value — might just be a
      // structural mismatch that the CSS diff already caught.
      continue;
    }

    const testForValue = testLabel.el.htmlAttributes?.["for"];
    if (!testForValue) {
      warnings.push(
        `label "${refEl.key}" — ref has for="${forValue}" but test label has no for attribute`
      );
      continue;
    }

    const testTarget = testIdMap.get(testForValue);

    // Case 1: Test label points to nothing.
    if (!testTarget && refTarget) {
      warnings.push(
        `label "${refEl.key}" for="${testForValue}" — no element with id="${testForValue}" in test ` +
        `(ref targets ${refTarget.tag})`
      );
      continue;
    }

    // Case 2: Both resolve but to different element types.
    if (refTarget && testTarget && refTarget.tag !== testTarget.tag) {
      const refType = describeTarget(refTarget);
      const testType = describeTarget(testTarget);
      warnings.push(
        `label "${refEl.key}" for="${forValue}" — ref targets ${refType} but test targets ${testType}`
      );
    }
  }

  return warnings;
}

/** Builds a map from id attribute value to the element snapshot. */
function buildIdMap(
  elements: ElementSnapshot[]
): Map<string, ElementSnapshot> {
  const map = new Map<string, ElementSnapshot>();
  for (const el of elements) {
    const id = el.htmlAttributes?.["id"];
    if (id) map.set(id, el);
  }
  return map;
}

/** Finds a test label that matches the ref label by text content or for value. */
function findMatchingLabel(
  refLabel: ElementSnapshot,
  testElements: ElementSnapshot[]
): { el: ElementSnapshot; index: number } | undefined {
  const refFor = refLabel.htmlAttributes?.["for"];

  // First pass: match by for value.
  for (let i = 0; i < testElements.length; i++) {
    const el = testElements[i]!;
    if (el.tag !== "LABEL") continue;
    if (el.htmlAttributes?.["for"] === refFor) return { el, index: i };
  }

  // Second pass: match by text content (key).
  for (let i = 0; i < testElements.length; i++) {
    const el = testElements[i]!;
    if (el.tag !== "LABEL") continue;
    if (el.key === refLabel.key) return { el, index: i };
  }

  return undefined;
}

/** Human-readable description of a label target. */
function describeTarget(el: ElementSnapshot): string {
  const type = el.htmlAttributes?.["type"];
  const role = el.role;

  if (el.tag === "INPUT" && type) return `<input type="${type}">`;
  if (role) return `<${el.tag.toLowerCase()} role="${role}">`;
  return `<${el.tag.toLowerCase()}>`;
}
