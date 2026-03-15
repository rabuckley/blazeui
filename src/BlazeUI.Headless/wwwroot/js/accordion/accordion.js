import { createKeyboardNavHandler } from "/_content/BlazeUI.Headless/blazeui.core.js";
import {
  registerDelegatedHandler,
  unregisterDelegatedHandler,
} from "/_content/BlazeUI.Bridge/blazeui.bridge.js";

const instances = new Map();

export function init(rootId, orientation, loopFocus) {
  const existing = instances.get(rootId);
  if (existing) {
    unregisterDelegatedHandler("keydown", rootId);
  }

  instances.set(rootId, { rootId });

  registerDelegatedHandler("keydown", rootId, createKeyboardNavHandler({
    orientation,
    loop: loopFocus,
    itemSelector: 'button[aria-expanded]:not([disabled]):not([data-disabled])',
  }));
}

// Called for both DefaultOpen (firstRender) and user-initiated opens.
// When suppressAnimation is true (DefaultOpen), cancels the entry animation.
// When false (user-initiated), restarts the CSS animation to work around
// Blazor Server's atomic render diff — the browser may skip the animation
// when display changes from none to visible in the same frame as the
// animation property is applied.
export function openPanel(panelId, suppressAnimation) {
  const panel = document.getElementById(panelId);
  if (!panel) return;

  panel.removeAttribute("hidden");

  if (suppressAnimation) {
    panel.getAnimations().forEach(a => a.cancel());
  } else {
    // Force-restart the CSS animation: clear it, trigger reflow, re-apply.
    panel.style.animation = "none";
    panel.offsetHeight;
    panel.style.animation = "";
  }
}

export function closePanel(panelId, dotNetRef) {
  const panel = document.getElementById(panelId);

  if (!panel) {
    dotNetRef?.invokeMethodAsync("OnCloseAnimationComplete", panelId);
    return;
  }

  // Blazor has already set data-closed on the panel, triggering the CSS
  // accordion-up keyframe animation. With `interpolate-size: allow-keywords`
  // (set in blazeui.css), the browser can animate from `auto` to `0` directly.
  // We just need to wait for the animation to finish, then hide the panel.

  const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const duration = reducedMotion ? 0 :
    (parseFloat(getComputedStyle(panel).animationDuration) * 1000 || 200);

  let done = false;
  const complete = () => {
    if (done) return;
    done = true;
    panel.setAttribute("hidden", "");
    dotNetRef?.invokeMethodAsync("OnCloseAnimationComplete", panelId);
  };

  setTimeout(complete, duration + 50);
  panel.addEventListener("animationend", complete, { once: true });
}

export function dispose(rootId) {
  const inst = instances.get(rootId);
  if (!inst) return;
  unregisterDelegatedHandler("keydown", rootId);
  instances.delete(rootId);
}
