import { chromium, type Page, type Browser } from "playwright";
import { interactionRegistry } from "../interactions/registry.js";
import { runInteractions } from "../interactions/runner.js";
import { unifiedDiff } from "../diff/text-diff.js";

interface InspectCommandOptions {
  blazor?: boolean;
  depth?: string;
  interact?: string;
  diff?: boolean;
  refBase?: string;
  testBase?: string;
  main?: boolean;
}

/**
 * Dumps the DOM HTML of a specific element for quick inspection.
 * Replaces the ad-hoc `node -e` + `page.evaluate(() => ...)` pattern.
 *
 * When --diff is set, the first positional arg is treated as a component name
 * and HTML is fetched from both ref and test apps, then diffed.
 */
export async function inspectCommand(
  urlOrComponent: string,
  selector: string,
  options: InspectCommandOptions
): Promise<void> {
  if (options.diff) {
    await inspectDiff(urlOrComponent, selector, options);
    return;
  }

  await inspectSingle(urlOrComponent, selector, options);
}

async function inspectSingle(
  url: string,
  selector: string,
  options: InspectCommandOptions
): Promise<void> {
  const depth = parseInt(options.depth ?? "0", 10);

  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();

  try {
    await page.goto(url, { waitUntil: "networkidle" });

    if (options.blazor) {
      await page.waitForFunction('typeof window.Blazor !== "undefined"', null, {
        timeout: 15_000,
      });
      await page.waitForTimeout(500);
    }

    if (options.interact) {
      const steps = interactionRegistry[options.interact];
      if (!steps) {
        console.error(
          `Unknown interaction: "${options.interact}". ` +
            `Available: ${Object.keys(interactionRegistry).join(", ")}`
        );
        process.exit(2);
      }
      await runInteractions(page, steps);
    }

    const scopeToMain = options.main !== false;
    const html = await extractHtml(page, selector, depth, scopeToMain);

    if (html === null) {
      console.error(`Element not found: ${selector}`);
      process.exit(1);
    }

    console.log(formatHtml(html));
  } finally {
    await page.close();
    await browser.close();
  }
}

async function inspectDiff(
  component: string,
  selector: string,
  options: InspectCommandOptions
): Promise<void> {
  const refBase = (options.refBase ?? "http://localhost:5200").replace(/\/$/, "");
  const testBase = (options.testBase ?? "http://127.0.0.1:5199").replace(/\/$/, "");
  const depth = parseInt(options.depth ?? "0", 10);

  const browser = await chromium.launch({ headless: true });

  try {
    const scopeToMain = options.main !== false;

    // Fetch HTML from reference app.
    const refHtml = await fetchHtml(
      browser,
      `${refBase}/${component}`,
      selector,
      depth,
      false,
      options.interact,
      scopeToMain
    );

    // Fetch HTML from test app (Blazor).
    const testHtml = await fetchHtml(
      browser,
      `${testBase}/${component}`,
      selector,
      depth,
      true,
      options.interact,
      scopeToMain
    );

    if (refHtml === null && testHtml === null) {
      console.error(`Element not found in either app: ${selector}`);
      process.exit(1);
    }
    if (refHtml === null) {
      console.error(`Element not found in ref app: ${selector}`);
      process.exit(1);
    }
    if (testHtml === null) {
      console.error(`Element not found in test app: ${selector}`);
      process.exit(1);
    }

    const refFormatted = formatHtml(refHtml);
    const testFormatted = formatHtml(testHtml);

    const refLines = refFormatted.split("\n");
    const testLines = testFormatted.split("\n");

    const diffLines = unifiedDiff(refLines, testLines);

    if (diffLines.length === 0) {
      console.log(`MATCH — HTML is identical for ${selector} on /${component}`);
    } else {
      console.log(`DIFF — ${selector} on /${component}`);
      console.log(`--- ref  (${refBase}/${component})`);
      console.log(`+++ test (${testBase}/${component})`);
      console.log();
      for (const line of diffLines) {
        console.log(line);
      }
    }
  } finally {
    await browser.close();
  }
}

async function fetchHtml(
  browser: Browser,
  url: string,
  selector: string,
  depth: number,
  blazor: boolean,
  interact?: string,
  scopeToMain: boolean = true
): Promise<string | null> {
  const page = await browser.newPage();

  try {
    await page.goto(url, { waitUntil: "networkidle" });

    if (blazor) {
      await page.waitForFunction('typeof window.Blazor !== "undefined"', null, {
        timeout: 15_000,
      });
      await page.waitForTimeout(500);
    }

    if (interact) {
      const steps = interactionRegistry[interact];
      if (!steps) {
        console.error(
          `Unknown interaction: "${interact}". ` +
            `Available: ${Object.keys(interactionRegistry).join(", ")}`
        );
        process.exit(2);
      }
      await runInteractions(page, steps);
    }

    return await extractHtml(page, selector, depth, scopeToMain);
  } finally {
    await page.close();
  }
}

async function extractHtml(
  page: Page,
  selector: string,
  depth: number,
  scopeToMain: boolean = true
): Promise<string | null> {
  return await page.evaluate(
    ({ sel, d, scoped }: { sel: string; d: number; scoped: boolean }) => {
      // When scoped, try finding the element within <main> first to avoid
      // matching sidebar/nav elements that share the same selector.
      let el: Element | null = null;
      if (scoped) {
        const main = document.querySelector("main");
        if (main) el = main.querySelector(sel);
      }
      if (!el) el = document.querySelector(sel);
      if (!el) return null;

      if (d >= 1 && el.parentElement) {
        return el.parentElement.innerHTML;
      }
      return el.outerHTML;
    },
    { sel: selector, d: depth, scoped: scopeToMain }
  );
}

/**
 * Naive HTML indentation for readability. Adds newlines and indentation
 * around tags for easier scanning. Not a full parser — just enough to
 * make DOM dumps readable.
 */
export function formatHtml(html: string): string {
  let indent = 0;
  const parts: string[] = [];

  // Split on tag boundaries while preserving tags.
  const tokens = html.split(/(<\/?[^>]+>)/g).filter(Boolean);

  for (const token of tokens) {
    const trimmed = token.trim();
    if (!trimmed) continue;

    if (trimmed.startsWith("</")) {
      // Closing tag — decrease indent before printing.
      indent = Math.max(0, indent - 1);
      parts.push("  ".repeat(indent) + trimmed);
    } else if (trimmed.startsWith("<") && !trimmed.endsWith("/>")) {
      // Opening tag.
      parts.push("  ".repeat(indent) + trimmed);
      indent++;
    } else if (trimmed.startsWith("<") && trimmed.endsWith("/>")) {
      // Self-closing tag.
      parts.push("  ".repeat(indent) + trimmed);
    } else {
      // Text content.
      parts.push("  ".repeat(indent) + trimmed);
    }
  }

  return parts.join("\n");
}
