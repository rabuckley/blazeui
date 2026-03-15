import type { ElementSnapshot, AlignmentEntry } from "../types.js";

/**
 * Aligns two element arrays using tag+role similarity matching to identify
 * where elements are missing or extra when counts differ.
 *
 * Walks both arrays in parallel. When tags+roles match, advances both
 * pointers. When they diverge, looks ahead (up to 5 elements) in each
 * array for a match to identify insertions/deletions.
 */
export function alignElements(
  refElements: ElementSnapshot[],
  testElements: ElementSnapshot[]
): AlignmentEntry[] {
  const result: AlignmentEntry[] = [];
  let ri = 0;
  let ti = 0;

  while (ri < refElements.length && ti < testElements.length) {
    const r = refElements[ri]!;
    const t = testElements[ti]!;

    if (matches(r, t)) {
      result.push({
        ref: { index: ri, tag: r.tag, role: r.role ?? "", key: r.key },
        test: { index: ti, tag: t.tag, role: t.role ?? "", key: t.key },
      });
      ri++;
      ti++;
      continue;
    }

    // Look ahead in test for a match to current ref element (ref has extra).
    const testAhead = lookAhead(r, testElements, ti, 5);
    // Look ahead in ref for a match to current test element (test has extra).
    const refAhead = lookAhead(t, refElements, ri, 5);

    if (refAhead !== -1 && (testAhead === -1 || refAhead <= testAhead)) {
      // Elements in ref before refAhead are missing from test.
      for (let i = ri; i < ri + refAhead; i++) {
        const el = refElements[i]!;
        result.push({
          ref: { index: i, tag: el.tag, role: el.role ?? "", key: el.key },
        });
      }
      ri += refAhead;
    } else if (testAhead !== -1) {
      // Elements in test before testAhead are extra (not in ref).
      for (let i = ti; i < ti + testAhead; i++) {
        const el = testElements[i]!;
        result.push({
          test: { index: i, tag: el.tag, role: el.role ?? "", key: el.key },
        });
      }
      ti += testAhead;
    } else {
      // No match found in lookahead — emit both as mismatched.
      result.push({
        ref: { index: ri, tag: r.tag, role: r.role ?? "", key: r.key },
        test: { index: ti, tag: t.tag, role: t.role ?? "", key: t.key },
      });
      ri++;
      ti++;
    }
  }

  // Remaining ref elements.
  while (ri < refElements.length) {
    const el = refElements[ri]!;
    result.push({
      ref: { index: ri, tag: el.tag, role: el.role ?? "", key: el.key },
    });
    ri++;
  }

  // Remaining test elements.
  while (ti < testElements.length) {
    const el = testElements[ti]!;
    result.push({
      test: { index: ti, tag: el.tag, role: el.role ?? "", key: el.key },
    });
    ti++;
  }

  return result;
}

function matches(a: ElementSnapshot, b: ElementSnapshot): boolean {
  return a.tag === b.tag && (a.role ?? "") === (b.role ?? "");
}

/**
 * Looks ahead in `elements` starting from `start` for an element matching
 * `target` by tag+role. Returns the offset from start, or -1 if not found.
 */
function lookAhead(
  target: ElementSnapshot,
  elements: ElementSnapshot[],
  start: number,
  maxLook: number
): number {
  const end = Math.min(start + maxLook, elements.length);
  for (let i = start; i < end; i++) {
    if (matches(target, elements[i]!)) {
      return i - start;
    }
  }
  return -1;
}
