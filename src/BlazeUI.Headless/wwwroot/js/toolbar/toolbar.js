import { createKeyboardNavHandler } from "/_content/BlazeUI.Headless/blazeui.core.js";
import {
  registerDelegatedHandler,
  unregisterDelegatedHandler,
} from "/_content/BlazeUI.Bridge/blazeui.bridge.js";

const instances = new Map();

export function init(toolbarId, orientation) {
  const existing = instances.get(toolbarId);
  if (existing) {
    unregisterDelegatedHandler("keydown", toolbarId);
  }

  instances.set(toolbarId, { toolbarId });

  registerDelegatedHandler("keydown", toolbarId, createKeyboardNavHandler({
    orientation,
    itemSelector: 'button:not([disabled]), a[href], input:not([disabled]), [tabindex]:not([tabindex="-1"])',
  }));
}

export function dispose(toolbarId) {
  const inst = instances.get(toolbarId);
  if (!inst) return;
  unregisterDelegatedHandler("keydown", toolbarId);
  instances.delete(toolbarId);
}
