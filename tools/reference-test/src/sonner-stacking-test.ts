/**
 * Playwright test that captures the Sonner toast stacking animation flow
 * on both the reference (React) and BlazeUI apps, then compares the
 * stacking properties to verify they match.
 *
 * Run from tools/reference-test/:
 *   pnpm tsx src/sonner-stacking-test.ts
 */

import { chromium, type Page } from "playwright";

const REF_BASE = "http://localhost:5200";
const BLAZE_BASE = "http://127.0.0.1:5199";

interface ToastSnapshot {
  text: string;
  dataIndex: string;
  dataFront: string;
  dataMounted: string;
  dataExpanded: string;
  dataVisible: string;
  toastsBefore: string;
  zIndex: string;
  offset: string;
  initialHeight: string;
  computedHeight: string;
  computedTransform: string;
  computedOpacity: string;
  boundingBottom: number;
}

interface ContainerSnapshot {
  frontToastHeight: string;
  gap: string;
  width: string;
}

interface StackSnapshot {
  container: ContainerSnapshot;
  toasts: ToastSnapshot[];
}

async function captureStackState(page: Page): Promise<StackSnapshot> {
  return page.evaluate(() => {
    const ol = document.querySelector("[data-sonner-toaster]")!;
    const toasts = document.querySelectorAll("[data-sonner-toast]");

    return {
      container: {
        frontToastHeight:
          ol.style.getPropertyValue("--front-toast-height") ?? "",
        gap: ol.style.getPropertyValue("--gap") ?? "",
        width: ol.style.getPropertyValue("--width") ?? "",
      },
      toasts: Array.from(toasts).map((t) => {
        const el = t as HTMLElement;
        const cs = getComputedStyle(el);
        return {
          text: (
            el.querySelector("[data-title]")?.textContent ?? ""
          ).substring(0, 40),
          dataIndex: el.dataset.index ?? "",
          dataFront: el.dataset.front ?? "",
          dataMounted: el.dataset.mounted ?? "",
          dataExpanded: el.dataset.expanded ?? "",
          dataVisible: el.dataset.visible ?? "",
          toastsBefore: el.style.getPropertyValue("--toasts-before"),
          zIndex: el.style.getPropertyValue("--z-index"),
          offset: el.style.getPropertyValue("--offset"),
          initialHeight: el.style.getPropertyValue("--initial-height"),
          computedHeight: cs.height,
          computedTransform: cs.transform,
          computedOpacity: cs.opacity,
          boundingBottom: el.getBoundingClientRect().bottom,
        };
      }),
    };
  });
}

function assertClose(
  actual: number,
  expected: number,
  tolerance: number,
  msg: string
) {
  if (Math.abs(actual - expected) > tolerance) {
    throw new Error(
      `${msg}: expected ~${expected}, got ${actual} (tolerance ${tolerance})`
    );
  }
}

function parseHeight(h: string): number {
  return parseFloat(h.replace("px", ""));
}

function parseTransformScale(t: string): number {
  // matrix(sx, 0, 0, sy, tx, ty)
  const m = t.match(/matrix\(([^,]+)/);
  return m ? parseFloat(m[1]) : 1;
}

function parseTransformTy(t: string): number {
  const parts = t.match(/matrix\(([^)]+)\)/);
  if (!parts) return 0;
  const vals = parts[1].split(",").map((v) => parseFloat(v.trim()));
  return vals[5]; // ty
}

async function runTest(
  label: string,
  baseUrl: string,
  waitMs: number
): Promise<StackSnapshot[]> {
  const browser = await chromium.launch();
  const page = await browser.newPage({ viewport: { width: 1280, height: 720 } });
  await page.goto(`${baseUrl}/toast`);
  await page.waitForTimeout(waitMs);

  const snapshots: StackSnapshot[] = [];

  // Phase 1: Click first button, capture after mount animation
  await page.click('button:has-text("Default")');
  await page.waitForTimeout(800);
  snapshots.push(await captureStackState(page));
  await page.screenshot({ path: `/tmp/${label}-phase1.png` });

  // Phase 2: Click second button, capture stacked state
  await page.click('button:has-text("Success")');
  await page.waitForTimeout(800);
  snapshots.push(await captureStackState(page));
  await page.screenshot({ path: `/tmp/${label}-phase2.png` });

  // Phase 3: Click third button, capture 3-toast stack
  await page.click('button:has-text("Info")');
  await page.waitForTimeout(800);
  snapshots.push(await captureStackState(page));
  await page.screenshot({ path: `/tmp/${label}-phase3.png` });

  // Phase 4: Hover over the front toast to expand the stack
  const frontToast = page.locator("[data-sonner-toast][data-front='true']");
  await frontToast.hover({ timeout: 5000 });
  await page.waitForTimeout(600);
  snapshots.push(await captureStackState(page));
  await page.screenshot({ path: `/tmp/${label}-phase4-expanded.png` });

  // Phase 5: Move away to collapse
  await page.mouse.move(100, 100);
  await page.waitForTimeout(600);
  snapshots.push(await captureStackState(page));
  await page.screenshot({ path: `/tmp/${label}-phase5-collapsed.png` });

  await browser.close();
  return snapshots;
}

function verifyStackingInvariants(label: string, snap: StackSnapshot) {
  const { toasts, container } = snap;

  // Container must have a non-zero front-toast-height
  const frontH = parseHeight(container.frontToastHeight);
  if (frontH <= 0) {
    throw new Error(
      `${label}: --front-toast-height is ${container.frontToastHeight}, expected > 0`
    );
  }

  // Front toast (index 0) checks
  const front = toasts[0];
  if (front.dataFront !== "true") {
    throw new Error(`${label}: toast[0] should be data-front=true`);
  }
  if (front.dataMounted !== "true") {
    throw new Error(`${label}: toast[0] should be data-mounted=true`);
  }

  // Check z-index ordering: front has highest z-index
  for (let i = 1; i < toasts.length; i++) {
    if (parseInt(toasts[i].zIndex) >= parseInt(front.zIndex)) {
      throw new Error(
        `${label}: toast[${i}] z-index (${toasts[i].zIndex}) >= front z-index (${front.zIndex})`
      );
    }
  }

  // Non-expanded, non-front toasts should be scaled down
  if (front.dataExpanded === "false") {
    for (let i = 1; i < toasts.length; i++) {
      const scale = parseTransformScale(toasts[i].computedTransform);
      const expectedScale = 1 - i * 0.05;
      assertClose(
        scale,
        expectedScale,
        0.01,
        `${label}: toast[${i}] scale`
      );

      // Should be translated upward (negative ty for bottom position)
      const ty = parseTransformTy(toasts[i].computedTransform);
      const expectedTy = -i * parseFloat(container.gap);
      assertClose(ty, expectedTy, 1, `${label}: toast[${i}] translateY`);

      // Non-front toasts should have height = front-toast-height
      const h = parseHeight(toasts[i].computedHeight);
      assertClose(h, frontH, 2, `${label}: toast[${i}] height`);
    }
  }
}

function compareSnapshots(
  refSnap: StackSnapshot,
  blazeSnap: StackSnapshot,
  phase: string
) {
  if (refSnap.toasts.length !== blazeSnap.toasts.length) {
    throw new Error(
      `${phase}: toast count mismatch — ref=${refSnap.toasts.length}, blaze=${blazeSnap.toasts.length}`
    );
  }

  for (let i = 0; i < refSnap.toasts.length; i++) {
    const ref = refSnap.toasts[i];
    const blaze = blazeSnap.toasts[i];

    // Compare scale
    const refScale = parseTransformScale(ref.computedTransform);
    const blazeScale = parseTransformScale(blaze.computedTransform);
    assertClose(blazeScale, refScale, 0.02, `${phase} toast[${i}] scale`);

    // Compare translateY
    const refTy = parseTransformTy(ref.computedTransform);
    const blazeTy = parseTransformTy(blaze.computedTransform);
    assertClose(blazeTy, refTy, 2, `${phase} toast[${i}] translateY`);

    // Compare data attributes
    if (ref.dataFront !== blaze.dataFront) {
      throw new Error(
        `${phase} toast[${i}] data-front: ref=${ref.dataFront}, blaze=${blaze.dataFront}`
      );
    }
    if (ref.dataExpanded !== blaze.dataExpanded) {
      throw new Error(
        `${phase} toast[${i}] data-expanded: ref=${ref.dataExpanded}, blaze=${blaze.dataExpanded}`
      );
    }
  }
}

async function main() {
  console.log("Running Sonner stacking animation test...\n");

  console.log("--- Reference (React Sonner) ---");
  const refSnapshots = await runTest("ref", REF_BASE, 500);

  console.log("--- BlazeUI (Sonner port) ---");
  const blazeSnapshots = await runTest("blaze", BLAZE_BASE, 1500);

  let passed = 0;
  let failed = 0;

  // Validate invariants for each phase
  const phases = [
    "1-toast",
    "2-toast stack",
    "3-toast stack",
    "expanded",
    "collapsed",
  ];
  for (let i = 0; i < phases.length; i++) {
    const phase = phases[i];
    try {
      verifyStackingInvariants(`ref ${phase}`, refSnapshots[i]);
      console.log(`  ✓ ref ${phase} invariants OK`);
      passed++;
    } catch (e: any) {
      console.log(`  ✗ ref ${phase}: ${e.message}`);
      failed++;
    }

    try {
      verifyStackingInvariants(`blaze ${phase}`, blazeSnapshots[i]);
      console.log(`  ✓ blaze ${phase} invariants OK`);
      passed++;
    } catch (e: any) {
      console.log(`  ✗ blaze ${phase}: ${e.message}`);
      failed++;
    }

    // Cross-compare ref vs blaze
    try {
      compareSnapshots(refSnapshots[i], blazeSnapshots[i], phase);
      console.log(`  ✓ ${phase} ref↔blaze match`);
      passed++;
    } catch (e: any) {
      console.log(`  ✗ ${phase} ref↔blaze: ${e.message}`);
      failed++;
    }
  }

  console.log(`\n${passed} passed, ${failed} failed`);

  // Print detailed state for debugging
  for (let i = 0; i < phases.length; i++) {
    console.log(`\n=== Phase: ${phases[i]} ===`);
    console.log(
      "REF container:",
      JSON.stringify(refSnapshots[i].container)
    );
    console.log(
      "BLAZE container:",
      JSON.stringify(blazeSnapshots[i].container)
    );
    for (let j = 0; j < refSnapshots[i].toasts.length; j++) {
      const r = refSnapshots[i].toasts[j];
      const b = blazeSnapshots[i].toasts[j];
      console.log(
        `  toast[${j}] ref:   front=${r.dataFront} expanded=${r.dataExpanded} h=${r.computedHeight} transform=${r.computedTransform}`
      );
      if (b) {
        console.log(
          `  toast[${j}] blaze: front=${b.dataFront} expanded=${b.dataExpanded} h=${b.computedHeight} transform=${b.computedTransform}`
        );
      }
    }
  }

  process.exit(failed > 0 ? 1 : 0);
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
