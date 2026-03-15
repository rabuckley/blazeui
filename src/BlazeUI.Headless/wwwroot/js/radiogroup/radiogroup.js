// RadioGroup keyboard navigation — arrow key nav with activate-on-focus,
// following WAI-ARIA radiogroup pattern. All four arrow keys are handled
// since RadioGroup doesn't expose an orientation parameter.
import { createKeyboardNavHandler } from "/_content/BlazeUI.Headless/blazeui.core.js";
import {
  registerDelegatedHandler,
  unregisterDelegatedHandler,
} from "/_content/BlazeUI.Bridge/blazeui.bridge.js";

const instances = new Map();

export function init(groupId) {
  const existing = instances.get(groupId);
  if (existing) {
    unregisterDelegatedHandler("keydown", groupId);
  }

  instances.set(groupId, { groupId });

  // No orientation — both horizontal and vertical arrows navigate.
  // activateOnFocus clicks the radio on focus (selects it).
  registerDelegatedHandler("keydown", groupId, createKeyboardNavHandler({
    itemSelector: '[role="radio"]:not([aria-disabled="true"])',
    activateOnFocus: true,
  }));
}

export function dispose(groupId) {
  const inst = instances.get(groupId);
  if (!inst) return;
  unregisterDelegatedHandler("keydown", groupId);
  instances.delete(groupId);
}
