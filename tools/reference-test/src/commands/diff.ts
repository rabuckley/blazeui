import { readFileSync } from "node:fs";
import type { StyleSnapshot, DiffReport } from "../types.js";
import { diffSnapshots } from "../diff/differ.js";
import { alignElements } from "../diff/aligner.js";
import { analyzeDataAttributeWarnings } from "../diff/behavioral.js";

type OutputFormat = "json" | "summary" | "detail";

interface DiffCommandOptions {
  format?: OutputFormat;
  resolved?: boolean;
}

export async function diffCommand(
  refPath: string,
  testPath: string,
  options: DiffCommandOptions
): Promise<void> {
  let ref: StyleSnapshot;
  let test: StyleSnapshot;

  try {
    ref = JSON.parse(readFileSync(refPath, "utf8")) as StyleSnapshot;
    test = JSON.parse(readFileSync(testPath, "utf8")) as StyleSnapshot;
  } catch (err) {
    console.error(`Failed to read snapshot files: ${err}`);
    process.exit(2);
  }

  const report = diffSnapshots(ref, test, undefined, {
    resolved: options.resolved,
  });

  if (options.format === "detail") {
    outputReport(report, "detail", ref, test);
  } else {
    outputReport(report, options.format ?? "json");
  }

  process.exit(report.status === "match" ? 0 : 1);
}

export function outputReport(
  report: DiffReport,
  format: OutputFormat,
  ref?: StyleSnapshot,
  test?: StyleSnapshot
): void {
  if (format === "summary") {
    outputSummary(report);
  } else if (format === "detail") {
    outputDetail(report, ref, test);
  } else {
    console.log(JSON.stringify(report, null, 2));
  }
}

function outputSummary(report: DiffReport): void {
  const { summary, elementCountDiff } = report;
  const suppressed = summary.suppressedCalcEquiv
    ? ` (${summary.suppressedCalcEquiv} custom-property-only suppressed)`
    : "";

  // Large element count deltas indicate structural divergence that
  // should never be hidden behind a MATCH label.
  const countDelta = elementCountDiff
    ? Math.abs(elementCountDiff.ref - elementCountDiff.test)
    : 0;

  const pseudoCount = report.pseudoStateDiffs?.length ?? 0;
  const pseudoLabel = pseudoCount > 0 ? `, ${pseudoCount} pseudo-state` : "";

  if (report.status === "match") {
    console.log(`MATCH /${report.component}${suppressed}`);
  } else if (summary.withDiffs === 0 && summary.keyMismatches > 0 && countDelta <= 2 && pseudoCount === 0) {
    // Minor key mismatches (e.g. SVG <path> vs <line>) with no structural
    // property diffs and near-identical element counts — safe to downgrade.
    const extra = elementCountDiff
      ? ` (elements: ref=${elementCountDiff.ref} test=${elementCountDiff.test})`
      : "";
    console.log(
      `MATCH /${report.component} (${summary.keyMismatches} layout-only diffs)${extra}${suppressed}`
    );
  } else if (summary.withDiffs === 0 && countDelta > 2) {
    // No CSS property diffs but significant element count mismatch —
    // likely a demo page structural issue hiding real diffs.
    const extra = ` (elements: ref=${elementCountDiff!.ref} test=${elementCountDiff!.test}, Δ${countDelta})`;
    console.log(
      `DIFF  /${report.component} — ${summary.keyMismatches} structural / ${summary.total} total${pseudoLabel}${extra}${suppressed}`
    );
  } else {
    const extra = elementCountDiff
      ? ` (elements: ref=${elementCountDiff.ref} test=${elementCountDiff.test})`
      : "";
    console.log(
      `DIFF  /${report.component} — ${summary.withDiffs} real / ${summary.total} total${pseudoLabel}${extra}${suppressed}`
    );
  }
}

function outputDetail(
  report: DiffReport,
  ref?: StyleSnapshot,
  test?: StyleSnapshot
): void {
  const { summary, elementCountDiff } = report;

  // Header with element count delta.
  const countInfo = elementCountDiff
    ? ` (ref=${elementCountDiff.ref} test=${elementCountDiff.test}, Δ${Math.abs(elementCountDiff.ref - elementCountDiff.test)})`
    : "";
  const statusTag = report.status === "match" ? "MATCH" : "DIFF";
  console.log(
    `${statusTag} /${report.component} — ${summary.withDiffs} real / ${summary.total} total${countInfo}`
  );

  if (summary.suppressedCalcEquiv) {
    console.log(
      `  (${summary.suppressedCalcEquiv} element${summary.suppressedCalcEquiv > 1 ? "s" : ""} with custom-property-only diffs suppressed)`
    );
  }

  if (report.status === "match") return;

  console.log();

  // Property diffs with structural context.
  for (const diff of report.diffs) {
    const classLabel = diff.classToken ? ` .${diff.classToken}` : "";
    const parentLabel =
      diff.parentTag && diff.parentDisplay
        ? `  (parent: ${diff.parentTag} ${diff.parentDisplay})`
        : "";
    console.log(`  [${diff.index}] ${diff.tag}${classLabel}${parentLabel}`);

    if (diff.role || diff.dataSlot) {
      const parts: string[] = [];
      if (diff.role) parts.push(`role=${diff.role}`);
      if (diff.dataSlot) parts.push(`slot=${diff.dataSlot}`);
      console.log(`      ${parts.join("  ")}`);
    }

    const props = Object.entries(diff.properties);
    for (let i = 0; i < props.length; i++) {
      const [prop, val] = props[i]!;
      const connector = i < props.length - 1 ? "├" : "└";
      // Pad property name to 16 chars for alignment.
      const label = (prop + ":").padEnd(17);
      console.log(`      ${connector} ${label}${val.ref} → ${val.test}`);
    }
    console.log();
  }

  // Key mismatches.
  if (report.keyMismatches.length > 0) {
    console.log(`  Key mismatches: ${report.keyMismatches.length}`);
    for (const km of report.keyMismatches) {
      console.log(
        `    [${km.index}] ref: "${km.refKey}" ${km.refTag} ≠ test: "${km.testKey}" ${km.testTag}`
      );
    }
    console.log();
  }

  // Behavioral warnings: Tailwind data-attribute variants targeting missing attributes.
  if (ref && test) {
    const warnings = analyzeDataAttributeWarnings(ref.elements, test.elements);
    if (warnings.length > 0) {
      console.log(`  Behavioral warnings:`);
      for (const w of warnings) {
        console.log(`    ${w}`);
      }
      console.log();
    }
  }

  // Pseudo-state diffs.
  if (report.pseudoStateDiffs && report.pseudoStateDiffs.length > 0) {
    // Group by state for readable output.
    const byState = new Map<string, typeof report.pseudoStateDiffs>();
    for (const entry of report.pseudoStateDiffs) {
      const group = byState.get(entry.state) ?? [];
      group.push(entry);
      byState.set(entry.state, group);
    }

    for (const [state, entries] of byState) {
      console.log(`  :${state} diffs:`);
      for (const entry of entries) {
        console.log(`    [${entry.elementIndex}] ${entry.tag} ${entry.key}`);

        const props = Object.entries(entry.properties);
        for (let i = 0; i < props.length; i++) {
          const [prop, val] = props[i]!;
          const connector = i < props.length - 1 ? "├" : "└";
          const label = (prop + ":").padEnd(17);
          console.log(`      ${connector} ${label}${val.ref} → ${val.test}`);
        }
      }
      console.log();
    }
  }

  // Element map around first divergence when counts differ.
  if (elementCountDiff && ref && test) {
    const alignment = alignElements(ref.elements, test.elements);

    // Find the first entry where one side is missing.
    const firstDivIdx = alignment.findIndex((e) => !e.ref || !e.test);
    if (firstDivIdx >= 0) {
      const start = Math.max(0, firstDivIdx - 2);
      const end = Math.min(alignment.length, firstDivIdx + 8);

      console.log(`  Element map (first divergence at index ${firstDivIdx}):`);
      for (let i = start; i < end; i++) {
        const entry = alignment[i]!;
        if (entry.ref && entry.test) {
          const rSlot = entry.ref.role ? `  role=${entry.ref.role}` : "";
          console.log(
            `    ref[${entry.ref.index}]  ${entry.ref.tag.padEnd(8)}${rSlot}    = test[${entry.test.index}]`
          );
        } else if (entry.ref && !entry.test) {
          const rSlot = entry.ref.role ? `  role=${entry.ref.role}` : "";
          console.log(
            `    ref[${entry.ref.index}]  ${entry.ref.tag.padEnd(8)}${rSlot}    ← missing in test`
          );
        } else if (entry.test && !entry.ref) {
          const tSlot = entry.test.role ? `  role=${entry.test.role}` : "";
          console.log(
            `    test[${entry.test.index}] ${entry.test.tag.padEnd(8)}${tSlot}    ← extra in test`
          );
        }
      }
      console.log();
    }
  }
}
