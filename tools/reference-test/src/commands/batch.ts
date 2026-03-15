import { chromium } from "playwright";
import { extractStyles, extractFromPage } from "../extract/extractor.js";
import { diffSnapshots } from "../diff/differ.js";
import { interactionRegistry } from "../interactions/registry.js";
import { runInteractions } from "../interactions/runner.js";
import { outputReport } from "./diff.js";
import type { DiffReport } from "../types.js";
import type { PseudoState } from "../extract/pseudo-states.js";

const ALL_COMPONENTS = [
  "button",
  "input",
  "checkbox",
  "switch",
  "toggle",
  "separator",
  "progress",
  "avatar",
  "slider",
  "radio",
  "toggle-group",
  "accordion",
  "tabs",
  "dialog",
  "alert-dialog",
  "drawer",
  "sheet",
  "select",
  "menu",
  "context-menu",
  "popover",
  "tooltip",
  "scroll-area",
  "sidebar",
  "collapsible",
  "combobox",
  "field",
  "preview-card",
  "toast",
  "menubar",
  "navigation-menu",
  "blocks/login-02",
  "blocks/sidebar-01",
  "blocks/sidebar-07",
];

interface BatchCommandOptions {
  refBase?: string;
  testBase?: string;
  format?: "json" | "summary" | "detail";
  resolved?: boolean;
  pseudoStates?: string;
}

export async function batchCommand(
  components: string[],
  options: BatchCommandOptions
): Promise<void> {
  const refBase = (options.refBase ?? "http://localhost:5200").replace(
    /\/$/,
    ""
  );
  const testBase = (options.testBase ?? "http://127.0.0.1:5199").replace(
    /\/$/,
    ""
  );
  const format = options.format ?? "summary";
  const comps = components.length > 0 ? components : ALL_COMPONENTS;

  const pseudoStates = options.pseudoStates
    ?.split(",")
    .map((s) => s.trim())
    .filter(Boolean) as PseudoState[] | undefined;

  const browser = await chromium.launch({ headless: true });
  let anyDiffs = false;
  let anyErrors = false;

  try {
    for (const comp of comps) {
      try {
        // Extract from reference app.
        const refResult = await extractStyles({
          url: `${refBase}/${comp}`,
          browser,
          pseudoStates,
        });

        // Run interactions on ref if defined, then re-extract.
        const steps = interactionRegistry[comp];
        if (steps) {
          await runInteractions(refResult.page, steps);
          refResult.snapshot.elements = await extractFromPage(refResult.page);
        }
        await refResult.page.close();

        // Extract from test app (Blazor).
        const testResult = await extractStyles({
          url: `${testBase}/${comp}`,
          blazor: true,
          browser,
          pseudoStates,
        });

        if (steps) {
          await runInteractions(testResult.page, steps);
          testResult.snapshot.elements = await extractFromPage(testResult.page);
        }
        await testResult.page.close();

        // Diff and report.
        const report = diffSnapshots(
          refResult.snapshot,
          testResult.snapshot,
          comp,
          { resolved: options.resolved }
        );
        if (report.status === "diff") anyDiffs = true;
        outputReport(report, format, refResult.snapshot, testResult.snapshot);
      } catch (err) {
        anyErrors = true;
        const report: DiffReport = {
          component: comp,
          status: "error",
          summary: { total: 0, keyMismatches: 0, withDiffs: 0 },
          keyMismatches: [],
          diffs: [],
        };
        if (format === "json") {
          console.log(JSON.stringify(report, null, 2));
        } else {
          console.log(`ERROR /${comp} — ${err}`);
        }
      }
    }
  } finally {
    await browser.close();
  }

  if (anyErrors) process.exit(2);
  if (anyDiffs) process.exit(1);
  process.exit(0);
}
