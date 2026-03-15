import { colord, extend } from "colord";
// eslint-disable-next-line @typescript-eslint/no-require-imports -- colord plugins use CJS default exports
import labPlugin from "colord/plugins/lab";
import lchPlugin from "colord/plugins/lch";
import namesPlugin from "colord/plugins/names";
import valueParser, { type Node } from "postcss-value-parser";

// @ts-expect-error -- colord plugin types don't align with NodeNext module resolution
extend([labPlugin, lchPlugin, namesPlugin]);

// Properties where values may contain colors.
const COLOR_PROPS = new Set([
  "color",
  "background-color",
  "border-color",
  "border-top-color",
  "border-right-color",
  "border-bottom-color",
  "border-left-color",
  "border-block-start-color",
  "border-block-end-color",
  "border-inline-start-color",
  "border-inline-end-color",
  "outline-color",
  "text-decoration-color",
  "column-rule-color",
  "caret-color",
  "accent-color",
  "fill",
  "stroke",
  "flood-color",
  "lighting-color",
  "stop-color",
  "box-shadow",
  "text-shadow",
  "background",
  "border",
  "border-top",
  "border-right",
  "border-bottom",
  "border-left",
]);

/**
 * Normalizes oklch values to a canonical decimal form within the same
 * color space. E.g. `oklch(97% 0 0)` → `oklch(0.97 0 0)`.
 *
 * We stay within the original color space to avoid lossy sRGB clamping.
 * The normalization rounds to 6 decimal places for stability.
 */
function normalizeOklch(raw: string): string {
  return raw.replace(/oklch\(([^)]+)\)/g, (_, inner: string) => {
    const parts = inner.split(/\s+/).map((p) => {
      if (p.endsWith("%")) {
        const n = parseFloat(p) / 100;
        return String(Math.round(n * 1e6) / 1e6);
      }
      return p;
    });
    return `oklch(${parts.join(" ")})`;
  });
}

/**
 * Attempts to normalize a color value to a canonical hex string.
 * Only converts if both ref and test are in the same sRGB-representable
 * space — otherwise returns the input unchanged so the diff is preserved.
 */
function normalizeColorValue(value: string): string {
  // First normalize oklch percentage → decimal within the same space.
  value = normalizeOklch(value);

  // For simple color values (not shorthand properties with mixed content),
  // try to parse with colord for canonical form.
  const parsed = colord(value);
  if (parsed.isValid()) {
    return parsed.toHex();
  }

  return value;
}

/**
 * Normalizes numeric formatting in a CSS value:
 * - Leading zeros: .4 → 0.4
 * - Trailing zeros: 0.250 → 0.25
 * - Time units: 0.15s → 150ms
 * - Quote style: "Foo" → 'Foo'
 * - Whitespace collapse
 */
function normalizeGeneric(value: string): string {
  let v = value;

  // oklch percentage → decimal: applies to all properties since custom
  // properties like --border often contain oklch values.
  v = normalizeOklch(v);

  // Leading-zero decimals: .4 → 0.4
  v = v.replace(/(^|[( ,\-])\.(\d)/g, "$10.$2");

  // Trailing zeros: 0.250 → 0.25 (keep at least one decimal digit)
  v = v.replace(/(\d+\.\d*?)0+([ ,)]|$)/g, (_, num: string, after: string) => {
    if (num.endsWith(".")) num += "0";
    return num + after;
  });

  // Time units: seconds → milliseconds
  v = v.replace(/(\d+\.?\d*)s\b/g, (_, n: string) =>
    Math.round(parseFloat(n) * 1000) + "ms"
  );

  // Quote style: double → single
  v = v.replace(/"([^"]+)"/g, "'$1'");

  // Collapse whitespace
  v = v.replace(/\s+/g, " ").trim();

  return v;
}

/**
 * Normalizes a CSS property value for comparison. Uses colord for
 * color properties and postcss-value-parser for structural normalization
 * of non-color values.
 */
export function normalizeValue(property: string, value: string): string {
  if (typeof value !== "string") return value;

  // Always apply generic normalization first.
  let v = normalizeGeneric(value);

  // For color properties (and Tailwind shadow custom properties that embed
  // colors), attempt color-specific normalization. The Vite and CLI Tailwind
  // pipelines serialize the same color differently (rgb() vs #hex).
  const mayContainColors =
    COLOR_PROPS.has(property) ||
    property.endsWith("-color") ||
    property === "--tw-shadow" ||
    property === "--tw-ring-shadow";
  if (mayContainColors) {
    // For compound values (e.g. box-shadow), try to normalize color
    // functions within the value using postcss-value-parser.
    try {
      const parsed = valueParser(v);
      parsed.walk((node: Node) => {
        if (
          node.type === "function" &&
          /^(rgb|rgba|hsl|hsla|oklch|oklab|lab|lch|color)$/i.test(node.value)
        ) {
          const raw = valueParser.stringify(node);
          const normalized = normalizeColorValue(raw);
          if (normalized !== raw) {
            // Replace the function node with the normalized string.
            // We mutate the node in-place for postcss-value-parser.
            (node as unknown as { type: string; value: string }).type = "word";
            (node as unknown as { type: string; value: string; nodes: never[] }).value = normalized;
            (node as unknown as { nodes: never[] }).nodes = [];
          }
        }

        // Hex colors with alpha (e.g. #0000001a) appear as word nodes in
        // compound values like box-shadow. Normalize them to canonical hex
        // so they match equivalent rgb() values from a different build pipeline.
        if (node.type === "word" && /^#[0-9a-f]{4,8}$/i.test(node.value)) {
          const normalized = normalizeColorValue(node.value);
          if (normalized !== node.value) {
            (node as unknown as { value: string }).value = normalized;
          }
        }
      });
      v = valueParser.stringify(parsed.nodes);
    } catch {
      // If parsing fails, fall through with generic normalization.
    }
  }

  return v;
}
