/**
 * Simple line-by-line unified diff using LCS (Longest Common Subsequence).
 * No external dependencies. Returns diff lines with +/- /space prefixes
 * and 3 lines of context around each change.
 */

/**
 * Computes the LCS table for two string arrays.
 */
function lcsTable(a: string[], b: string[]): number[][] {
  const m = a.length;
  const n = b.length;
  const dp: number[][] = Array.from({ length: m + 1 }, () =>
    new Array(n + 1).fill(0)
  );

  for (let i = 1; i <= m; i++) {
    for (let j = 1; j <= n; j++) {
      if (a[i - 1] === b[j - 1]) {
        dp[i]![j] = dp[i - 1]![j - 1]! + 1;
      } else {
        dp[i]![j] = Math.max(dp[i - 1]![j]!, dp[i]![j - 1]!);
      }
    }
  }

  return dp;
}

interface DiffOp {
  type: "equal" | "delete" | "insert";
  line: string;
}

/**
 * Backtracks the LCS table to produce a sequence of diff operations.
 */
function backtrack(dp: number[][], a: string[], b: string[]): DiffOp[] {
  const ops: DiffOp[] = [];
  let i = a.length;
  let j = b.length;

  while (i > 0 || j > 0) {
    if (i > 0 && j > 0 && a[i - 1] === b[j - 1]) {
      ops.push({ type: "equal", line: a[i - 1]! });
      i--;
      j--;
    } else if (j > 0 && (i === 0 || dp[i]![j - 1]! >= dp[i - 1]![j]!)) {
      ops.push({ type: "insert", line: b[j - 1]! });
      j--;
    } else {
      ops.push({ type: "delete", line: a[i - 1]! });
      i--;
    }
  }

  return ops.reverse();
}

/**
 * Produces a unified diff between two string arrays with context lines.
 *
 * @param refLines - Lines from the reference (labelled with -)
 * @param testLines - Lines from the test (labelled with +)
 * @param contextLines - Number of context lines around each change (default: 3)
 * @returns Array of formatted diff lines with +/-/space prefixes
 */
export function unifiedDiff(
  refLines: string[],
  testLines: string[],
  contextLines = 3
): string[] {
  const dp = lcsTable(refLines, testLines);
  const ops = backtrack(dp, refLines, testLines);

  // Find which ops are changes (not equal).
  const changeIndices = ops
    .map((op, i) => (op.type !== "equal" ? i : -1))
    .filter((i) => i >= 0);

  if (changeIndices.length === 0) return [];

  // Build context-aware hunks: include contextLines around each change.
  const include = new Set<number>();
  for (const idx of changeIndices) {
    for (let c = Math.max(0, idx - contextLines); c <= Math.min(ops.length - 1, idx + contextLines); c++) {
      include.add(c);
    }
  }

  const result: string[] = [];
  let lastIncluded = -2;

  for (let i = 0; i < ops.length; i++) {
    if (!include.has(i)) continue;

    // Insert a separator when there's a gap in included lines.
    if (lastIncluded >= 0 && i - lastIncluded > 1) {
      result.push("---");
    }
    lastIncluded = i;

    const op = ops[i]!;
    switch (op.type) {
      case "equal":
        result.push(` ${op.line}`);
        break;
      case "delete":
        result.push(`-${op.line}`);
        break;
      case "insert":
        result.push(`+${op.line}`);
        break;
    }
  }

  return result;
}
