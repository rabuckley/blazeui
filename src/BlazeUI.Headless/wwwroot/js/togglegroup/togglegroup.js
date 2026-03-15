// ToggleGroup keyboard navigation — orientation-aware arrow key nav
// with roving tabindex, following the WAI-ARIA toolbar pattern.
import { createKeyboardNavHandler } from "/_content/BlazeUI.Headless/blazeui.core.js";
import {
  registerDelegatedHandler,
  unregisterDelegatedHandler,
} from "/_content/BlazeUI.Bridge/blazeui.bridge.js";

const instances = new Map();

export function init(groupId, orientation) {
  const existing = instances.get(groupId);
  if (existing) {
    unregisterDelegatedHandler("keydown", groupId);
  }

  instances.set(groupId, { groupId });

  // Set initial roving tabindex: only the first (or pressed) button is tabbable.
  const group = document.getElementById(groupId);
  if (group) {
    applyRovingTabindex(group);
  }

  registerDelegatedHandler("keydown", groupId, createKeyboardNavHandler({
    orientation,
    itemSelector: 'button:not([disabled])',
    focusItem: focusItem,
  }));
}

// Roving tabindex: only the focused item has tabindex="0", others get "-1".
function focusItem(item, items) {
  for (const i of items) {
    i.setAttribute("tabindex", "-1");
  }
  item.setAttribute("tabindex", "0");
  item.focus();
}

function applyRovingTabindex(group) {
  const items = [...group.querySelectorAll(
    'button:not([disabled])'
  )].filter(el => !el.closest("[hidden]") && el.offsetParent !== null);
  if (items.length === 0) return;

  // The pressed item (or first item if none pressed) gets tabindex="0".
  const pressed = items.find(item => item.hasAttribute("data-pressed"));
  const tabbable = pressed ?? items[0];

  for (const item of items) {
    item.setAttribute("tabindex", item === tabbable ? "0" : "-1");
  }
}

export function dispose(groupId) {
  const inst = instances.get(groupId);
  if (!inst) return;
  unregisterDelegatedHandler("keydown", groupId);
  instances.delete(groupId);
}
