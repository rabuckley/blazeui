import { writeFileSync } from "node:fs";
import { extractStyles, extractFromPage } from "../extract/extractor.js";
import { interactionRegistry } from "../interactions/registry.js";
import { runInteractions } from "../interactions/runner.js";
import type { PseudoState } from "../extract/pseudo-states.js";

interface ExtractCommandOptions {
  blazor?: boolean;
  interact?: string;
  scope?: string;
  pseudoStates?: string;
  output?: string;
}

export async function extractCommand(
  url: string,
  options: ExtractCommandOptions
): Promise<void> {
  const pseudoStates = options.pseudoStates
    ?.split(",")
    .map((s) => s.trim())
    .filter(Boolean) as PseudoState[] | undefined;

  const { snapshot, page, browser } = await extractStyles({
    url,
    blazor: options.blazor,
    scope: options.scope,
    pseudoStates,
  });

  // Run interaction steps if requested, then re-extract.
  if (options.interact) {
    const steps = interactionRegistry[options.interact];
    if (!steps) {
      console.error(
        `Unknown interaction: "${options.interact}". ` +
          `Available: ${Object.keys(interactionRegistry).join(", ")}`
      );
      await page.close();
      await browser.close();
      process.exit(2);
    }

    await runInteractions(page, steps);
    snapshot.elements = await extractFromPage(page, options.scope);
  }

  await page.close();
  await browser.close();

  const json = JSON.stringify(snapshot, null, 2);
  if (options.output) {
    writeFileSync(options.output, json, "utf8");
    console.error(`Wrote ${snapshot.elements.length} elements to ${options.output}`);
  } else {
    process.stdout.write(json + "\n");
  }
}
