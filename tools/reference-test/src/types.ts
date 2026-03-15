/** A single element's computed CSS properties. */
export interface ElementSnapshot {
  /** Human-readable key: direct text content, slot name, or tag name. */
  key: string;
  /** HTML tag name or pseudo-element (e.g. "BUTTON", "::before"). */
  tag: string;
  /** All computed CSS property values, plus _w/_h for bounding rect. */
  props: Record<string, string>;
  /** Index of the nearest ancestor in the result array, or -1 if none. */
  parentIndex?: number;
  /** ARIA role attribute value. */
  role?: string;
  /** data-slot attribute value. */
  dataSlot?: string;
  /** CSS className string. */
  className?: string;
  /** All data-* attributes on the element (key without "data-" prefix). */
  dataAttributes?: Record<string, string>;
  /**
   * Semantic HTML attributes that affect behavior but aren't data-* or CSS.
   * Captured for behavioral analysis (label-for wiring, keyboard nav, etc.).
   */
  htmlAttributes?: Record<string, string>;
}

/** A property delta for a single element under a forced pseudo-state. */
export interface PseudoStateDelta {
  elementIndex: number;
  key: string;
  tag: string;
  changedProps: Record<string, string>;
}

/** Pseudo-state extraction result for one state (e.g. :hover). */
export interface PseudoStateResult {
  state: string;
  deltas: PseudoStateDelta[];
}

/** Full extraction from a single page. */
export interface StyleSnapshot {
  url: string;
  component: string;
  timestamp: string;
  elements: ElementSnapshot[];
  pseudoStates?: Record<string, PseudoStateDelta[]>;
}

/** A single property difference between ref and test. */
export interface PropertyDiff {
  ref: string;
  test: string;
}

/** A diff entry for a matched element pair. */
export interface ElementDiff {
  index: number;
  key: string;
  tag: string;
  properties: Record<string, PropertyDiff>;
  /** ARIA role from the ref element. */
  role?: string;
  /** data-slot from the ref element. */
  dataSlot?: string;
  /** First CSS class token from the ref element. */
  classToken?: string;
  /** Parent element's tag name. */
  parentTag?: string;
  /** Parent element's CSS display value. */
  parentDisplay?: string;
}

/** A diff entry where element keys don't match at a position. */
export interface KeyMismatch {
  index: number;
  refKey: string;
  testKey: string;
  refTag: string;
  testTag: string;
}

/** Summary counts for a diff report. */
export interface DiffSummary {
  /** Total element pairs compared. */
  total: number;
  /** Positions where keys didn't match. */
  keyMismatches: number;
  /** Elements with at least one property diff. */
  withDiffs: number;
  /** Elements suppressed because only calc-equivalent custom properties differed. */
  suppressedCalcEquiv?: number;
}

/** Options controlling diff behavior. */
export interface DiffOptions {
  /** Skip all CSS custom properties, compare only resolved values. */
  resolved?: boolean;
}

/** Configuration for multi-instance isolation testing. */
export interface IsolationConfig {
  /** CSS selector to find all instances. */
  selector: string;
  /** Attribute to read as the component's state. */
  stateAttr: string;
  /**
   * Optional CSS selector relative to each instance to read state from.
   * When set, state is read from the first child matching this selector
   * rather than from the instance element itself.
   */
  stateSelector?: string;
  /** How to interact with an instance. */
  action: "click" | "drag";
  /** Fraction of element width to drag (for drag action). */
  dragOffset?: number;
}

/** Result of isolation testing for a component. */
export interface IsolationResult {
  component: string;
  status: "pass" | "fail" | "error" | "skipped";
  instanceCount: number;
  details: IsolationDetail[];
}

/** Detail for a single isolation interaction. */
export interface IsolationDetail {
  interactedIndex: number;
  changed: boolean;
  /** Whether the instance was detected as disabled. */
  disabled?: boolean;
  leakedIndices: number[];
  states: { before: string[]; after: string[] };
}

/** A diff entry for a pseudo-state property mismatch between ref and test. */
export interface PseudoStateDiffEntry {
  state: string;
  elementIndex: number;
  key: string;
  tag: string;
  properties: Record<string, PropertyDiff>;
}

/** Full diff report for a component. */
export interface DiffReport {
  component: string;
  status: "match" | "diff" | "error";
  summary: DiffSummary;
  elementCountDiff?: { ref: number; test: number };
  keyMismatches: KeyMismatch[];
  diffs: ElementDiff[];
  pseudoStateDiffs?: PseudoStateDiffEntry[];
}

/** An interaction step to run before extraction. */
export interface InteractionStep {
  action: "click" | "rightclick" | "hover";
  selector: string;
  /** Zero-based index when `selector` matches multiple elements. Defaults to 0 (first). */
  nth?: number;
  /** Action to reverse the interaction (e.g. close after open). */
  reverseAction?: "click" | "rightclick" | "hover";
  /** Selector for the reverse action. Defaults to selector if omitted. */
  reverseSelector?: string;
  /** Element to measure during animation sampling. */
  targetSelector?: string;
  /** Selector for a second item to click during switch-mode animation testing. */
  switchSelector?: string;
  /** Element to measure as the opening target during switch-mode animation. */
  switchTargetSelector?: string;
  /** Skip the open-position check (for centered overlays like dialog/alert-dialog). */
  skipOpenPos?: boolean;
}

/** A single animation sample captured via requestAnimationFrame. */
export interface AnimationSample {
  timeMs: number;
  height: number;
  display: string;
  opacity: number;
  visibility: string;
  /** Bounding rect x (left edge). Present when position tracking is enabled. */
  x?: number;
  /** Bounding rect y (top edge). */
  y?: number;
}

/** Animation trace from one app. */
export interface AnimationTrace {
  app: "ref" | "test";
  samples: AnimationSample[];
  durationMs: number;
}

/** Result of checking for close-animation rebound (height drops to ~0 then jumps back). */
export interface ReboundResult {
  rebounded: boolean;
  reboundHeight?: number;
  reboundAtMs?: number;
}

/** Animation comparison report for close mode. */
export interface AnimationReport {
  component: string;
  mode: "close";
  status: "match" | "diff" | "error";
  ref: AnimationTrace;
  test: AnimationTrace;
  rebound?: ReboundResult;
}

/** Animation comparison report for switch mode (simultaneous close + open). */
export interface SwitchAnimationReport {
  component: string;
  mode: "switch";
  status: "match" | "diff" | "error";
  closing: { ref: AnimationTrace; test: AnimationTrace };
  opening: { ref: AnimationTrace; test: AnimationTrace };
}

/** Alignment entry for element map in detail format. */
export interface AlignmentEntry {
  ref?: { index: number; tag: string; role: string; key: string };
  test?: { index: number; tag: string; role: string; key: string };
}

/** Result of a close→open race condition resilience check. */
export interface ReopenResult {
  status: "pass" | "fail" | "error" | "skipped";
  visible?: boolean;
  positionStable?: boolean;
  details?: string;
}

/** Unified verification result for a single component. */
export interface VerifyResult {
  component: string;
  css: { status: "match" | "diff" | "error"; diffCount: number };
  animation?: { status: "match" | "diff" | "error" | "skipped"; mode?: "close" | "switch" };
  switchAnimation?: { status: "match" | "diff" | "error" | "skipped" };
  closeRebound?: { status: "pass" | "fail" | "error" | "skipped" };
  openPosition?: { status: "pass" | "fail" | "error" | "skipped" };
  reopen?: { status: "pass" | "fail" | "error" | "skipped" };
  behavioral?: { warnings: string[] };
  isolation?: { status: "pass" | "fail" | "error" | "skipped" };
  pseudoStates?: { status: "match" | "diff" | "error" | "skipped"; diffCount: number };
  overall: "pass" | "warn" | "fail" | "error";
}
