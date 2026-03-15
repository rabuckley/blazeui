import type { Page } from "playwright";
import type { AnimationSample, AnimationTrace, ReboundResult } from "../types.js";
import { evaluateString, escapeSelector } from "./evaluate.js";

/**
 * Captures animation samples on a target element using a requestAnimationFrame
 * loop. The observer is installed *before* the triggering action so the first
 * frame is not missed.
 *
 * Two-phase approach:
 * 1. Install rAF observer that records samples into window.__animSamples
 * 2. Caller triggers the action (click/etc) — browser rAF callbacks run
 *    between CDP commands, capturing frames throughout the animation
 * 3. Wait for timeout, then collect samples
 */
export async function installAnimationObserver(
  page: Page,
  targetSelector: string
): Promise<void> {
  const sel = escapeSelector(targetSelector);
  await evaluateString(page, `(() => {
    window.__animSamples = [];
    window.__animDone = false;

    var start = performance.now();
    var sel = '${sel}';

    var sample = function() {
      if (window.__animDone) return;

      var target = document.querySelector(sel);
      if (!target) {
        window.__animSamples.push({
          timeMs: Math.round(performance.now() - start),
          height: 0,
          display: 'none',
          opacity: 0,
          visibility: 'hidden',
          x: 0,
          y: 0,
        });
        requestAnimationFrame(sample);
        return;
      }

      var cs = getComputedStyle(target);
      var rect = target.getBoundingClientRect();

      window.__animSamples.push({
        timeMs: Math.round(performance.now() - start),
        height: Math.round(rect.height),
        display: cs.display,
        opacity: parseFloat(cs.opacity),
        visibility: cs.visibility,
        x: Math.round(rect.x),
        y: Math.round(rect.y),
      });

      requestAnimationFrame(sample);
    };

    requestAnimationFrame(sample);
  })()`);
}

/**
 * Collects animation samples after the observer has been running.
 * Signals the rAF loop to stop and retrieves all recorded samples.
 */
export async function collectAnimationSamples(
  page: Page,
  timeoutMs: number
): Promise<AnimationSample[]> {
  await page.waitForTimeout(timeoutMs);

  const samples = await evaluateString<AnimationSample[]>(page, `(() => {
    window.__animDone = true;
    return window.__animSamples;
  })()`);

  return samples ?? [];
}

/**
 * Installs a dual animation observer that samples two elements simultaneously
 * (one closing, one opening) in a single rAF loop. This ensures timestamps
 * are directly comparable between the two traces.
 *
 * Used for switch-mode animation testing where clicking a new item causes
 * the previous item to close and the new one to open concurrently.
 */
export async function installDualAnimationObserver(
  page: Page,
  closingSelector: string,
  openingSelector: string
): Promise<void> {
  const closeSel = escapeSelector(closingSelector);
  const openSel = escapeSelector(openingSelector);
  await evaluateString(page, `(() => {
    window.__animSamplesClose = [];
    window.__animSamplesOpen = [];
    window.__animDone = false;

    var start = performance.now();
    var closeSel = '${closeSel}';
    var openSel = '${openSel}';

    var sample = function() {
      if (window.__animDone) return;
      var now = Math.round(performance.now() - start);

      var closeEl = document.querySelector(closeSel);
      if (closeEl) {
        var cs = getComputedStyle(closeEl);
        var rect = closeEl.getBoundingClientRect();
        window.__animSamplesClose.push({
          timeMs: now,
          height: Math.round(rect.height),
          display: cs.display,
          opacity: parseFloat(cs.opacity),
          visibility: cs.visibility,
        });
      } else {
        window.__animSamplesClose.push({
          timeMs: now, height: 0, display: 'none', opacity: 0, visibility: 'hidden',
        });
      }

      var openEl = document.querySelector(openSel);
      if (openEl) {
        var cs2 = getComputedStyle(openEl);
        var rect2 = openEl.getBoundingClientRect();
        window.__animSamplesOpen.push({
          timeMs: now,
          height: Math.round(rect2.height),
          display: cs2.display,
          opacity: parseFloat(cs2.opacity),
          visibility: cs2.visibility,
        });
      } else {
        window.__animSamplesOpen.push({
          timeMs: now, height: 0, display: 'none', opacity: 0, visibility: 'hidden',
        });
      }

      requestAnimationFrame(sample);
    };

    requestAnimationFrame(sample);
  })()`);
}

/**
 * Collects dual animation samples (closing + opening) after the observer
 * has been running. Returns both sample arrays.
 */
export async function collectDualAnimationSamples(
  page: Page,
  timeoutMs: number
): Promise<{ closing: AnimationSample[]; opening: AnimationSample[] }> {
  await page.waitForTimeout(timeoutMs);

  const result = await evaluateString<{
    closing: AnimationSample[];
    opening: AnimationSample[];
  }>(page, `(() => {
    window.__animDone = true;
    return {
      closing: window.__animSamplesClose || [],
      opening: window.__animSamplesOpen || [],
    };
  })()`);

  return result ?? { closing: [], opening: [] };
}

/**
 * Builds an AnimationTrace from collected samples.
 */
export function buildTrace(
  app: "ref" | "test",
  samples: AnimationSample[]
): AnimationTrace {
  if (samples.length === 0) {
    return { app, samples: [], durationMs: 0 };
  }

  const first = samples[0]!;
  const last = samples[samples.length - 1]!;
  const durationMs = last.timeMs - first.timeMs;

  return { app, samples, durationMs };
}

/**
 * Classifies an animation trace as "smooth" or "instant" based on
 * the number of distinct height values observed.
 */
export function classifyAnimation(trace: AnimationTrace): "smooth" | "instant" | "none" {
  if (trace.samples.length === 0) return "none";

  const heights = new Set(trace.samples.map((s) => s.height));
  // A smooth animation should have at least 3 distinct height values.
  return heights.size >= 3 ? "smooth" : "instant";
}

/**
 * Compares the end-state of two close-animation traces. Returns true if
 * both traces end in a similar state (both hidden or both still visible).
 * Catches the case where ref ends at 0px (element removed/hidden) but
 * test stays at its original height (close animation had no effect).
 */
export function compareEndState(
  ref: AnimationTrace,
  test: AnimationTrace
): { match: boolean; refEndRatio: number; testEndRatio: number } {
  if (ref.samples.length === 0 || test.samples.length === 0) {
    return { match: true, refEndRatio: 0, testEndRatio: 0 };
  }

  const refStart = ref.samples[0]!.height;
  const refEnd = ref.samples[ref.samples.length - 1]!.height;
  const testStart = test.samples[0]!.height;
  const testEnd = test.samples[test.samples.length - 1]!.height;

  // Ratio of end height to start height. 0 = fully hidden, 1 = unchanged.
  const refEndRatio = refStart > 0 ? refEnd / refStart : 0;
  const testEndRatio = testStart > 0 ? testEnd / testStart : 0;

  // If one trace ends hidden (<20% of start) and the other stays visible
  // (>80% of start), the trajectories are fundamentally different.
  const refHidden = refEndRatio < 0.2;
  const testHidden = testEndRatio < 0.2;
  const match = refHidden === testHidden;

  return { match, refEndRatio: Math.round(refEndRatio * 100) / 100, testEndRatio: Math.round(testEndRatio * 100) / 100 };
}

/**
 * Detects close-animation rebound: the popup height drops to near-zero
 * then jumps back up, causing a visible flash. This is a Blazor-specific
 * lifecycle bug where StateHasChanged re-renders the popup before the
 * CSS transition completes.
 */
export function detectRebound(samples: AnimationSample[]): ReboundResult {
  const NEAR_ZERO_THRESHOLD = 5;

  let reachedZeroIndex = -1;
  for (let i = 0; i < samples.length; i++) {
    if (samples[i]!.height <= NEAR_ZERO_THRESHOLD) {
      reachedZeroIndex = i;
      break;
    }
  }

  if (reachedZeroIndex === -1) {
    return { rebounded: false };
  }

  // Check if any subsequent sample bounces back above the threshold.
  for (let i = reachedZeroIndex + 1; i < samples.length; i++) {
    const sample = samples[i]!;
    if (sample.height > NEAR_ZERO_THRESHOLD) {
      return {
        rebounded: true,
        reboundHeight: sample.height,
        reboundAtMs: sample.timeMs,
      };
    }
  }

  return { rebounded: false };
}

/**
 * Detects post-animation opacity flash: opacity reaches near-zero during
 * the exit animation, then snaps back to ~1.0 before the element unmounts.
 * This is caused by the CSS animation fill-mode expiring before Blazor
 * removes the element from the DOM (a single-frame flash at full opacity).
 */
export function detectOpacityFlash(samples: AnimationSample[]): ReboundResult {
  const NEAR_ZERO = 0.05;
  const FLASH_THRESHOLD = 0.5;

  let reachedZeroIndex = -1;
  for (let i = 0; i < samples.length; i++) {
    if (samples[i]!.opacity <= NEAR_ZERO && samples[i]!.height > 0) {
      reachedZeroIndex = i;
      break;
    }
  }

  if (reachedZeroIndex === -1) {
    return { rebounded: false };
  }

  // Check if any subsequent VISIBLE sample has high opacity (flash).
  for (let i = reachedZeroIndex + 1; i < samples.length; i++) {
    const sample = samples[i]!;
    if (sample.height > 0 && sample.opacity > FLASH_THRESHOLD) {
      return {
        rebounded: true,
        reboundHeight: sample.height,
        reboundAtMs: sample.timeMs,
      };
    }
  }

  return { rebounded: false };
}

/**
 * Checks that the first visible frame of an open animation starts near
 * the anchor element, not at (0, 0) or some other wrong position.
 * Returns the distance in pixels from the expected anchor-relative position.
 */
export function checkOpenPosition(
  samples: AnimationSample[],
  anchorRect: { x: number; y: number; width: number; height: number }
): { ok: boolean; firstX: number; firstY: number; distance: number } {
  // A frame is "visually visible" when it has non-zero height AND non-zero
  // opacity. Before JS sets data-open, the popup may exist in the DOM
  // (height > 0) but be invisible (opacity 0 from CSS animation initial state).
  const firstVisible = samples.find(
    (s) => s.height > 0 && s.opacity > 0 && s.x !== undefined && s.y !== undefined
  );

  if (!firstVisible) {
    return { ok: false, firstX: 0, firstY: 0, distance: Infinity };
  }

  // The popup should appear near the anchor's bottom edge (for bottom placement).
  // Allow generous tolerance since scale animation shifts the bounding rect.
  const expectedX = anchorRect.x;
  const expectedY = anchorRect.y + anchorRect.height;
  const dx = Math.abs(firstVisible.x! - expectedX);
  const dy = Math.abs(firstVisible.y! - expectedY);
  const distance = Math.sqrt(dx * dx + dy * dy);

  // 100px tolerance accounts for scale animation offset + different placements.
  return {
    ok: distance < 100,
    firstX: firstVisible.x!,
    firstY: firstVisible.y!,
    distance: Math.round(distance),
  };
}
