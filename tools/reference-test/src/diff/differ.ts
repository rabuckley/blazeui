import type {
  StyleSnapshot,
  DiffReport,
  ElementDiff,
  KeyMismatch,
  PropertyDiff,
  PseudoStateDiffEntry,
  PseudoStateDelta,
  DiffOptions,
} from "../types.js";
import { normalizeValue } from "./normalizer.js";

/**
 * Compares two style snapshots element-by-element using positional matching.
 *
 * Elements are matched by array index — same approach as the original
 * diff-styles.mjs. This works because both apps render equivalent page
 * structures where element order corresponds across ref and test.
 */
export function diffSnapshots(
  ref: StyleSnapshot,
  test: StyleSnapshot,
  component?: string,
  options?: DiffOptions
): DiffReport {
  const name = component ?? ref.component ?? "unknown";
  const keyMismatches: KeyMismatch[] = [];
  const diffs: ElementDiff[] = [];
  let suppressedCalcEquiv = 0;

  const len = Math.min(ref.elements.length, test.elements.length);

  for (let i = 0; i < len; i++) {
    const r = ref.elements[i]!;
    const t = test.elements[i]!;

    // Key mismatch — elements at this position have different identities.
    // Still compare a small set of structural properties so real diffs
    // (e.g. display: flex → block) aren't hidden by the skip.
    if (r.key !== t.key) {
      keyMismatches.push({
        index: i,
        refKey: r.key,
        testKey: t.key,
        refTag: r.tag,
        testTag: t.tag,
      });

      // Structural property comparison on key-mismatched elements.
      // These are the properties most likely to reveal real component
      // bugs vs positional noise from element count differences.
      const structuralProps = ["display", "position", "flex-direction"];
      const structDiffs: Record<string, PropertyDiff> = {};
      for (const prop of structuralProps) {
        const rv = normalizeValue(prop, r.props[prop] ?? "");
        const tv = normalizeValue(prop, t.props[prop] ?? "");
        if (rv !== tv) {
          structDiffs[prop] = { ref: r.props[prop] ?? "", test: t.props[prop] ?? "" };
        }
      }
      if (Object.keys(structDiffs).length > 0) {
        const parentEl = r.parentIndex != null && r.parentIndex >= 0
          ? ref.elements[r.parentIndex] : undefined;
        diffs.push({
          index: i,
          key: `${r.key} ≠ ${t.key}`,
          tag: `${r.tag}/${t.tag}`,
          properties: structDiffs,
          role: r.role || undefined,
          dataSlot: r.dataSlot || undefined,
          classToken: r.className?.split(/\s+/)[0] || undefined,
          parentTag: parentEl?.tag,
          parentDisplay: parentEl?.props["display"],
        });
      }

      continue;
    }

    // Compare all properties.
    const rp = r.props;
    const tp = t.props;
    const allKeys = new Set([...Object.keys(rp), ...Object.keys(tp)]);
    const properties: Record<string, PropertyDiff> = {};

    for (const prop of allKeys) {
      // --resolved mode: skip ALL custom properties unconditionally.
      if (options?.resolved && prop.startsWith("--")) continue;

      // Skip CSS custom properties where one build defines them and the
      // other doesn't. These are inherited Tailwind v4 theme variables
      // (e.g. --aspect-video, --blur-xs, --color-border) that differ
      // between builds because each build only emits variables for
      // utilities actually used. With @theme inline the values are
      // inlined into utility classes, so the rendered styles are
      // identical — the custom property is just a side-effect.
      if (prop.startsWith("--")) {
        const rv = rp[prop] ?? "";
        const tv = tp[prop] ?? "";
        if (rv === "" || tv === "") continue;
      }

      const rv = normalizeValue(prop, rp[prop] ?? "");
      const tv = normalizeValue(prop, tp[prop] ?? "");
      if (rv !== tv) {
        properties[prop] = { ref: rp[prop] ?? "", test: tp[prop] ?? "" };
      }
    }

    // Auto-suppress custom-property-only diffs.
    // If the only diffs on an element are CSS custom properties and all
    // resolved properties match, the visual output is identical — the
    // custom property values are intermediate/runtime noise (e.g.
    // calc() theme expressions, JS-set scroll offsets).
    const propKeys = Object.keys(properties);
    if (propKeys.length > 0) {
      const hasResolved = propKeys.some((p) => !p.startsWith("--"));

      if (!hasResolved) {
        suppressedCalcEquiv++;
        continue;
      }
    }

    if (propKeys.length > 0) {
      // Populate structural context from the ref element for detail output.
      const parentEl = r.parentIndex != null && r.parentIndex >= 0
        ? ref.elements[r.parentIndex] : undefined;

      diffs.push({
        index: i,
        key: r.key,
        tag: r.tag,
        properties,
        role: r.role || undefined,
        dataSlot: r.dataSlot || undefined,
        classToken: r.className?.split(/\s+/)[0] || undefined,
        parentTag: parentEl?.tag,
        parentDisplay: parentEl?.props["display"],
      });
    }
  }

  const elementCountDiff =
    ref.elements.length !== test.elements.length
      ? { ref: ref.elements.length, test: test.elements.length }
      : undefined;

  // Pseudo-state diffs.
  const pseudoStateDiffs = diffPseudoStates(ref, test);

  const hasDiffs =
    diffs.length > 0 || keyMismatches.length > 0 || !!elementCountDiff ||
    pseudoStateDiffs.length > 0;

  return {
    component: name,
    status: hasDiffs ? "diff" : "match",
    summary: {
      total: len,
      keyMismatches: keyMismatches.length,
      withDiffs: diffs.length,
      ...(suppressedCalcEquiv > 0 ? { suppressedCalcEquiv } : {}),
    },
    elementCountDiff,
    keyMismatches,
    diffs,
    ...(pseudoStateDiffs.length > 0 ? { pseudoStateDiffs } : {}),
  };
}

/**
 * Compares pseudo-state deltas between ref and test snapshots.
 *
 * For each pseudo-state present in either snapshot, pairs deltas by
 * elementIndex and diffs changedProps using normalizeValue(). Returns
 * entries only where properties actually differ between ref and test.
 */
function diffPseudoStates(
  ref: StyleSnapshot,
  test: StyleSnapshot
): PseudoStateDiffEntry[] {
  const refPs = ref.pseudoStates ?? {};
  const testPs = test.pseudoStates ?? {};
  const allStates = new Set([...Object.keys(refPs), ...Object.keys(testPs)]);

  const entries: PseudoStateDiffEntry[] = [];

  for (const state of allStates) {
    const refDeltas = refPs[state] ?? [];
    const testDeltas = testPs[state] ?? [];

    // Index deltas by elementIndex for efficient pairing.
    const refByIdx = new Map<number, PseudoStateDelta>();
    for (const d of refDeltas) refByIdx.set(d.elementIndex, d);

    const testByIdx = new Map<number, PseudoStateDelta>();
    for (const d of testDeltas) testByIdx.set(d.elementIndex, d);

    const allIndices = new Set([...refByIdx.keys(), ...testByIdx.keys()]);

    for (const idx of allIndices) {
      const rd = refByIdx.get(idx);
      const td = testByIdx.get(idx);

      const refChanged = rd?.changedProps ?? {};
      const testChanged = td?.changedProps ?? {};
      const allProps = new Set([...Object.keys(refChanged), ...Object.keys(testChanged)]);

      const properties: Record<string, PropertyDiff> = {};
      for (const prop of allProps) {
        // Skip internal size props — bounding rect may shift under pseudo-states
        // due to layout reflow, not styling differences.
        if (prop === "_w" || prop === "_h") continue;

        const rv = normalizeValue(prop, refChanged[prop] ?? "");
        const tv = normalizeValue(prop, testChanged[prop] ?? "");
        if (rv !== tv) {
          properties[prop] = {
            ref: refChanged[prop] ?? "(unchanged)",
            test: testChanged[prop] ?? "(unchanged)",
          };
        }
      }

      if (Object.keys(properties).length > 0) {
        // Use ref delta metadata if available, fall back to test.
        const source = rd ?? td!;
        entries.push({
          state,
          elementIndex: idx,
          key: source.key,
          tag: source.tag,
          properties,
        });
      }
    }
  }

  return entries;
}
