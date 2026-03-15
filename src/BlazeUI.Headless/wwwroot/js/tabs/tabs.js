import { createKeyboardNavHandler } from "/_content/BlazeUI.Headless/blazeui.core.js";
import {
  registerDelegatedHandler,
  unregisterDelegatedHandler,
} from "/_content/BlazeUI.Bridge/blazeui.bridge.js";

const instances = new Map();

// activateOnFocus: when true, arrow key navigation immediately activates the focused tab.
//   When false (default), focus moves without activating — Enter/Space is required.
// loopFocus: when true (default), focus wraps from the last tab back to the first and vice versa.
export function init(tabListId, orientation, activateOnFocus = false, loopFocus = true) {
  const existing = instances.get(tabListId);
  if (existing) {
    unregisterDelegatedHandler("keydown", tabListId);
  }

  instances.set(tabListId, { tabListId, orientation, activateOnFocus, loopFocus });

  registerDelegatedHandler("keydown", tabListId, createKeyboardNavHandler({
    orientation,
    loop: loopFocus,
    activateOnFocus,
    itemSelector: '[role="tab"]:not([disabled])',
  }));
}

export function measureIndicator(tabListId, activeTabValue) {
  const tabList = document.getElementById(tabListId);
  if (!tabList) return;

  const activeTab = tabList.querySelector(`[data-value="${activeTabValue}"]`);
  if (!activeTab) return;

  const listRect = tabList.getBoundingClientRect();
  const tabRect = activeTab.getBoundingClientRect();

  tabList.style.setProperty("--active-tab-left", `${tabRect.left - listRect.left}px`);
  tabList.style.setProperty("--active-tab-top", `${tabRect.top - listRect.top}px`);
  tabList.style.setProperty("--active-tab-width", `${tabRect.width}px`);
  tabList.style.setProperty("--active-tab-height", `${tabRect.height}px`);
}

export function dispose(tabListId) {
  const inst = instances.get(tabListId);
  if (!inst) return;
  unregisterDelegatedHandler("keydown", tabListId);
  instances.delete(tabListId);
}
