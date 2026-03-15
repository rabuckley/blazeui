// Comprehensive navigation menu animation test.
// Tests box (viewport/popup) and content panel animations during
// open, close, and directional content switches.
//
// Key insight: BlazeUI hover triggers go through Blazor Server round-trip
// (200ms enter delay + ~20ms SignalR). The test waits for state changes
// before sampling animations.

import { chromium } from "playwright";

const REF_URL = "http://localhost:5200/navigation-menu";
const BLAZE_URL = "http://127.0.0.1:5199/navigation-menu";
const TRIGGER_SEL = "[data-slot=navigation-menu-trigger]";
const CONTENT_SEL = "[data-slot=navigation-menu-content]";

function buildSampler() {
  return `(() => {
    let box = document.querySelector('[data-slot=navigation-menu-viewport]');
    if (!box) {
      const c = document.querySelector('${CONTENT_SEL}');
      if (c) {
        let el = c.parentElement;
        while (el && el !== document.body) {
          if (el.style.getPropertyValue('--popup-height') || el.style.getPropertyValue('--popup-width')) { box = el; break; }
          el = el.parentElement;
        }
      }
    }
    const rect = box ? box.getBoundingClientRect() : { width: 0, height: 0 };
    const cs = box ? getComputedStyle(box) : {};
    const boxS = {
      w: Math.round(rect.width * 10) / 10,
      h: Math.round(rect.height * 10) / 10,
      opacity: parseFloat(cs.opacity || '0'),
      transform: cs.transform || 'none',
    };
    const contents = document.querySelectorAll('${CONTENT_SEL}');
    const contentS = Array.from(contents).map(c => {
      const s = getComputedStyle(c);
      return {
        open: c.hasAttribute('data-open'),
        closed: c.hasAttribute('data-closed'),
        dir: c.getAttribute('data-activation-direction'),
        opacity: parseFloat(s.opacity || '0'),
        translate: s.translate || 'none',
      };
    });
    return { box: boxS, contents: contentS };
  })()`;
}

async function waitForContent(page, count = 1, timeout = 2000) {
  const start = Date.now();
  while (Date.now() - start < timeout) {
    const n = await page.evaluate((sel) => document.querySelectorAll(sel).length, CONTENT_SEL);
    if (n >= count) return true;
    await page.waitForTimeout(50);
  }
  return false;
}

async function waitForBoxSize(page, minW = 10, timeout = 2000) {
  const start = Date.now();
  while (Date.now() - start < timeout) {
    const w = await page.evaluate(() => {
      let box = document.querySelector('[data-slot=navigation-menu-viewport]');
      if (!box) {
        const c = document.querySelector('[data-slot=navigation-menu-content]');
        if (c) { let el = c.parentElement; while (el && el !== document.body) { if (el.style.getPropertyValue('--popup-height')) { box = el; break; } el = el.parentElement; } }
      }
      return box ? box.getBoundingClientRect().width : 0;
    });
    if (w >= minW) return true;
    await page.waitForTimeout(16);
  }
  return false;
}

async function captureFrames(page, count = 30) {
  const frames = [];
  for (let i = 0; i < count; i++) {
    frames.push(await page.evaluate(buildSampler()));
    await page.waitForTimeout(16);
  }
  return frames;
}

function analyse(frames, accessor) {
  const vals = frames.map(accessor).filter(v => v !== null && v !== undefined);
  if (vals.length === 0) return { first: null, last: null, unique: 0, range: 0, changing: false, smooth: false };
  const first = vals[0], last = vals[vals.length - 1];
  const numVals = vals.filter(v => typeof v === "number");
  const unique = [...new Set(numVals.map(v => Math.round(v * 100) / 100))].length;
  const range = numVals.length ? Math.max(...numVals) - Math.min(...numVals) : 0;
  return { first, last, unique, range: Math.round(range * 10) / 10, changing: first !== last, smooth: unique > 3 && range > 2 };
}

function desc(a) {
  if (a.unique === 0) return "N/A";
  if (!a.changing && typeof a.first === "number") return `${a.first.toFixed(1)} (static)`;
  if (!a.changing) return `${a.first} (static)`;
  const f = typeof a.first === "number" ? a.first.toFixed(1) : a.first;
  const l = typeof a.last === "number" ? a.last.toFixed(1) : a.last;
  return `${f}→${l} ${a.smooth ? "smooth" : "abrupt"} (${a.unique} steps, Δ${a.range})`;
}

async function test(url, name) {
  const browser = await chromium.launch();
  const page = await browser.newPage({ viewport: { width: 1280, height: 720 } });
  await page.goto(url, { waitUntil: "networkidle" });
  await page.waitForTimeout(500);
  const results = {};

  // === 1. Open (hover trigger-0) ===
  console.log(`\n[${name}] 1. Open`);
  await page.locator(TRIGGER_SEL).first().hover({ force: true });
  await waitForContent(page);
  // Now capture the mid-animation frames
  const s1 = await captureFrames(page, 25);
  results.open = s1;
  console.log(`  box.w:      ${desc(analyse(s1, f => f.box.w))}`);
  console.log(`  box.h:      ${desc(analyse(s1, f => f.box.h))}`);
  console.log(`  box.opacity:${desc(analyse(s1, f => f.box.opacity))}`);
  console.log(`  content.opacity: ${desc(analyse(s1, f => f.contents.find(c => c.open)?.opacity))}`);
  await waitForBoxSize(page);
  await page.waitForTimeout(400);

  // === 2. Switch L→R (trigger-0 → trigger-1) ===
  console.log(`[${name}] 2. Switch L→R`);
  await page.locator(TRIGGER_SEL).nth(1).hover({ force: true });
  await page.waitForTimeout(250); // enter delay
  const s2 = await captureFrames(page, 30);
  results.switchLR = s2;
  console.log(`  box.w:      ${desc(analyse(s2, f => f.box.w))}`);
  console.log(`  box.h:      ${desc(analyse(s2, f => f.box.h))}`);
  console.log(`  enter.opacity:   ${desc(analyse(s2, f => f.contents.find(c => c.open)?.opacity))}`);
  console.log(`  enter.translate: ${desc(analyse(s2, f => f.contents.find(c => c.open)?.translate))}`);
  console.log(`  exit.opacity:    ${desc(analyse(s2, f => f.contents.find(c => c.closed)?.opacity))}`);
  console.log(`  exit.translate:  ${desc(analyse(s2, f => f.contents.find(c => c.closed)?.translate))}`);
  await page.waitForTimeout(600);

  // === 3. Switch R→L (trigger-1 → trigger-0) ===
  console.log(`[${name}] 3. Switch R→L`);
  await page.locator(TRIGGER_SEL).first().hover({ force: true });
  await page.waitForTimeout(250);
  const s3 = await captureFrames(page, 30);
  results.switchRL = s3;
  console.log(`  box.w:      ${desc(analyse(s3, f => f.box.w))}`);
  console.log(`  box.h:      ${desc(analyse(s3, f => f.box.h))}`);
  console.log(`  enter.opacity:   ${desc(analyse(s3, f => f.contents.find(c => c.open)?.opacity))}`);
  console.log(`  exit.opacity:    ${desc(analyse(s3, f => f.contents.find(c => c.closed)?.opacity))}`);
  console.log(`  exit.translate:  ${desc(analyse(s3, f => f.contents.find(c => c.closed)?.translate))}`);
  await page.waitForTimeout(600);

  // === 4. Jump switch (trigger-0 → trigger-2) ===
  console.log(`[${name}] 4. Jump L→R (0→2)`);
  await page.locator(TRIGGER_SEL).nth(2).hover({ force: true });
  await page.waitForTimeout(250);
  const s4 = await captureFrames(page, 30);
  results.jump = s4;
  console.log(`  box.w:      ${desc(analyse(s4, f => f.box.w))}`);
  console.log(`  box.h:      ${desc(analyse(s4, f => f.box.h))}`);
  console.log(`  enter.opacity:   ${desc(analyse(s4, f => f.contents.find(c => c.open)?.opacity))}`);
  console.log(`  exit.opacity:    ${desc(analyse(s4, f => f.contents.find(c => c.closed)?.opacity))}`);
  console.log(`  exit.translate:  ${desc(analyse(s4, f => f.contents.find(c => c.closed)?.translate))}`);
  await page.waitForTimeout(600);

  // === 5. Close (move away) ===
  console.log(`[${name}] 5. Close`);
  await page.mouse.move(700, 600);
  await page.waitForTimeout(350); // exit delay
  const s5 = await captureFrames(page, 30);
  results.close = s5;
  console.log(`  box.w:      ${desc(analyse(s5, f => f.box.w))}`);
  console.log(`  box.h:      ${desc(analyse(s5, f => f.box.h))}`);
  console.log(`  box.opacity:${desc(analyse(s5, f => f.box.opacity))}`);

  await browser.close();
  return results;
}

function compare(r, b, label, accessor) {
  const ra = analyse(r, accessor);
  const ba = analyse(b, accessor);
  const ok = ra.changing === ba.changing && (!ra.changing || ra.smooth === ba.smooth);
  return { label, ref: ra, blaze: ba, ok };
}

async function main() {
  console.log("Navigation Menu Animation Test\n");
  const [ref, blaze] = await Promise.all([
    test(REF_URL, "REF"),
    test(BLAZE_URL, "BLAZE"),
  ]);

  console.log("\n========== COMPARISON ==========\n");
  const checks = [];
  const scenarios = [
    { key: "open", label: "Open", props: [["box.w", f => f.box.w], ["box.h", f => f.box.h], ["box.opacity", f => f.box.opacity], ["content.opacity", f => f.contents.find(c => c.open)?.opacity]] },
    { key: "switchLR", label: "Switch L→R", props: [["box.w", f => f.box.w], ["box.h", f => f.box.h], ["enter.opacity", f => f.contents.find(c => c.open)?.opacity], ["exit.opacity", f => f.contents.find(c => c.closed)?.opacity], ["exit.translate", f => f.contents.find(c => c.closed)?.translate]] },
    { key: "switchRL", label: "Switch R→L", props: [["box.w", f => f.box.w], ["box.h", f => f.box.h], ["exit.opacity", f => f.contents.find(c => c.closed)?.opacity], ["exit.translate", f => f.contents.find(c => c.closed)?.translate]] },
    { key: "jump", label: "Jump", props: [["box.w", f => f.box.w], ["box.h", f => f.box.h], ["exit.opacity", f => f.contents.find(c => c.closed)?.opacity]] },
    { key: "close", label: "Close", props: [["box.w", f => f.box.w], ["box.h", f => f.box.h], ["box.opacity", f => f.box.opacity]] },
  ];

  for (const { key, label, props } of scenarios) {
    console.log(`${label}:`);
    for (const [prop, acc] of props) {
      const c = compare(ref[key], blaze[key], `${label} ${prop}`, acc);
      checks.push(c);
      console.log(`  ${c.ok ? "✓" : "✗"} ${prop}: ref=[${desc(c.ref)}] blaze=[${desc(c.blaze)}]`);
    }
    console.log("");
  }

  const failures = checks.filter(c => !c.ok);
  console.log(`RESULT: ${failures.length === 0 ? "PASS" : `FAIL (${failures.length}/${checks.length})`}`);
  if (failures.length) {
    console.log("\nFailing:");
    failures.forEach(f => console.log(`  ${f.label}: ref=[${desc(f.ref)}] blaze=[${desc(f.blaze)}]`));
  }
  process.exit(failures.length === 0 ? 0 : 1);
}

main().catch(e => { console.error(e); process.exit(1); });
