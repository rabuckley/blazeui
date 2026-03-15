import type { InteractionStep, IsolationConfig } from "../types.js";

/**
 * Declarative map of component names to the interaction steps needed
 * to reveal their interactive/open state before extracting styles.
 *
 * These selectors target the trigger elements that open overlays,
 * expand accordions, switch tabs, etc. The optional targetSelector
 * identifies the element to measure during animation sampling.
 */
export const interactionRegistry: Record<string, InteractionStep[]> = {
  accordion: [
    {
      action: "click",
      selector: "h3 button",
      targetSelector: "[role=region]",
      switchSelector: "h3:nth-of-type(2) button",
      switchTargetSelector: "h3:nth-of-type(2) + [role=region]",
    },
  ],
  collapsible: [
    {
      action: "click",
      selector: "button",
      targetSelector: "[data-slot=collapsible-content]",
    },
  ],
  tabs: [{ action: "click", selector: "[role=tab]:nth-of-type(2)" }],
  dialog: [
    {
      action: "click",
      selector: "button[aria-haspopup=dialog]",
      reverseSelector: "[data-slot=dialog-close]",
      targetSelector: ":is([role=dialog], dialog[open])",
      skipOpenPos: true,
    },
  ],
  "alert-dialog": [
    {
      action: "click",
      selector: "button[aria-haspopup=dialog]",
      reverseSelector: ":is([role=alertdialog], dialog) [data-slot=alert-dialog-cancel]",
      targetSelector: ":is([role=alertdialog], dialog[open])",
      skipOpenPos: true,
    },
  ],
  drawer: [
    {
      action: "click",
      selector: "button[aria-haspopup='dialog']",
      reverseSelector: ":is([data-state='open'], [data-open]).fixed.inset-0",
      targetSelector: "[role=dialog]",
      skipOpenPos: true,
    },
  ],
  sheet: [
    {
      action: "click",
      selector: "button[aria-haspopup=dialog]",
      reverseSelector: "[data-slot=sheet-close]",
      targetSelector: "[role=dialog]",
      skipOpenPos: true,
    },
  ],
  select: [
    {
      action: "click",
      selector: "[role=combobox]",
      reverseSelector: "body",
      targetSelector: "[role=listbox]",
    },
  ],
  menu: [
    {
      action: "click",
      selector: "button",
      reverseSelector: "body",
      targetSelector: "[role=menu]",
    },
  ],
  "context-menu": [
    {
      action: "rightclick",
      selector: "[data-testid='context-menu-area'], .flex.items-center.justify-center",
      reverseSelector: "body",
      targetSelector: "[role=menu]",
    },
  ],
  popover: [
    {
      action: "click",
      selector: "button",
      targetSelector: "[data-slot=popover-content]",
    },
  ],
  tooltip: [{ action: "hover", selector: "button" }],
  combobox: [
    {
      action: "click",
      selector: "input[role=combobox]",
      targetSelector: "[role=listbox]",
    },
  ],
  toast: [
    {
      action: "click",
      selector: "button",
      targetSelector: "[data-sonner-toast]",
    },
  ],
  menubar: [
    {
      action: "click",
      selector: "[data-slot=menubar-trigger]",
      reverseSelector: "body",
      targetSelector: "[role=menu]",
    },
  ],
  "navigation-menu": [
    {
      action: "click",
      selector: "[data-slot=navigation-menu-trigger]",
      targetSelector: "[data-slot=navigation-menu-viewport]",
    },
  ],
  "preview-card": [
    {
      action: "hover",
      selector: "[data-slot=hover-card-trigger]",
      targetSelector: "[data-slot=hover-card-content]",
    },
  ],
  "blocks/sidebar-01": [
    // Open each dropdown to mount all three portal containers.
    // BlazeUI only renders portal content on first open, while the
    // reference keeps closed portals in the DOM. Opening each trigger
    // in sequence mounts all portals so element counts match.
    {
      action: "click",
      selector: "[data-slot=dropdown-menu-trigger]",
      nth: 0,
      reverseSelector: "body",
      targetSelector: "[role=menu]",
    },
    {
      action: "click",
      selector: "[data-slot=dropdown-menu-trigger]",
      nth: 1,
    },
    {
      action: "click",
      selector: "[data-slot=dropdown-menu-trigger]",
      nth: 2,
    },
  ],
  "blocks/sidebar-07": [
    // Same portal-mounting pattern as sidebar-01.
    {
      action: "click",
      selector: "[data-slot=dropdown-menu-trigger]",
      nth: 0,
      reverseSelector: "body",
      targetSelector: "[role=menu]",
    },
    {
      action: "click",
      selector: "[data-slot=dropdown-menu-trigger]",
      nth: 1,
    },
    {
      action: "click",
      selector: "[data-slot=dropdown-menu-trigger]",
      nth: 2,
    },
  ],
};

/**
 * Components that render multiple instances on the same page.
 * Used by the `isolate` command to verify instances don't share JS state.
 */
export const isolationRegistry: Record<string, IsolationConfig> = {
  slider: {
    // Use the slider root (role=group) as the drag target — it's wide enough
    // for meaningful drags. Read state from the hidden input inside each root.
    selector: "[role=group][data-slot=slider], [role=group]:has(input[type=range])",
    stateAttr: "aria-valuenow",
    stateSelector: "input[type=range]",
    action: "drag",
    dragOffset: 0.3,
  },
  accordion: {
    selector: "h3 button",
    stateAttr: "aria-expanded",
    action: "click",
  },
  tabs: {
    selector: "[role=tab]",
    stateAttr: "aria-selected",
    action: "click",
  },
};
