// PreviewCard JS — hover-intent + positioning + animation.
// Follows the same pattern as tooltip.js: the positioner is the floating container
// and the popup is the animated child inside it.
import {
  onHoverIntent,
  onEscapeKey,
  startAutoUpdate,
  stopAutoUpdate,
  animateEntry,
  animateExitCancellable,
  showPortalEntry,
  hidePortalEntry,
} from "/_content/BlazeUI.Headless/blazeui.core.js";
import {
  registerDelegatedHandler,
  unregisterDelegatedHandler,
} from "/_content/BlazeUI.Bridge/blazeui.bridge.js";

// Per-instance state keyed by triggerId. Supports multiple preview cards on the
// same page without module-level variable collisions.
const instances = new Map();

// Registers hover intent on a trigger using document-level event delegation so
// the listeners survive Blazor's SSR → interactive DOM replacement. Uses
// pointerover/pointerout (which bubble) instead of pointerenter/pointerleave
// (which don't) and tracks enter/leave state to filter child transitions.
function registerDelegatedHoverIntent(triggerId, options, dotNetRef, enterMethod, exitMethod) {
  const enterDelay = options.enterDelay ?? 300;
  const exitDelay = options.exitDelay ?? 0;
  let enterTimer = null;
  let exitTimer = null;
  let isInside = false;

  const clearTimers = () => {
    clearTimeout(enterTimer);
    clearTimeout(exitTimer);
  };

  const scheduleOpen = () => {
    clearTimers();
    enterTimer = setTimeout(() => {
      dotNetRef.invokeMethodAsync(enterMethod);
    }, enterDelay);
  };

  const scheduleClose = () => {
    clearTimers();
    exitTimer = setTimeout(() => {
      isInside = false;
      dotNetRef.invokeMethodAsync(exitMethod);
    }, exitDelay);
  };

  registerDelegatedHandler("pointerover", triggerId, () => {
    if (!isInside) {
      isInside = true;
      scheduleOpen();
    }
  });

  registerDelegatedHandler("pointerout", triggerId, (e, el) => {
    if (!el.contains(e.relatedTarget)) {
      scheduleClose();
    }
  });

  registerDelegatedHandler("focusin", triggerId, () => scheduleOpen());
  registerDelegatedHandler("focusout", triggerId, () => scheduleClose());

  return () => {
    clearTimers();
    unregisterDelegatedHandler("pointerover", triggerId);
    unregisterDelegatedHandler("pointerout", triggerId);
    unregisterDelegatedHandler("focusin", triggerId);
    unregisterDelegatedHandler("focusout", triggerId);
  };
}

export function init(cfg, ref) {
  const state = {
    hoverCleanup: null,
    escapeCleanup: null,
    autoUpdateId: null,
    config: cfg,
    dotNetRef: ref,
  };
  instances.set(cfg.triggerId, state);

  // Popup may not exist yet (it mounts when open). Hover intent works on
  // trigger alone initially; positioner hover is wired in positionAndShow().
  // Uses delegated handlers so the listeners survive SSR → interactive handoff.
  state.hoverCleanup = registerDelegatedHoverIntent(
    cfg.triggerId,
    { enterDelay: cfg.enterDelay, exitDelay: cfg.exitDelay },
    ref,
    "OnHoverEnter",
    "OnHoverExit"
  );
}

// positionerId: the element Floating UI positions (position: absolute container).
// Hover intent is also wired to this element so hovering the popup keeps it open.
export function positionAndShow(triggerId, positionerId, options) {
  const state = instances.get(triggerId);
  if (!state) return;

  const trigger = document.getElementById(triggerId);
  const positioner = document.getElementById(positionerId);
  if (!trigger || !positioner) return;

  // Start auto-positioning the positioner relative to trigger.
  if (state.autoUpdateId) stopAutoUpdate(state.autoUpdateId);
  state.autoUpdateId = startAutoUpdate(trigger, positioner, options);

  // Animate the popup child (the div inside the positioner).
  // JS owns data-open/data-closed — Blazor does not render these attributes.
  const popup = positioner.firstElementChild;
  if (popup) {
    showPortalEntry(popup);
    popup.setAttribute("data-open", "");
    animateEntry(popup);
  }

  // Wire escape key on document so it fires regardless of focus position.
  if (state.escapeCleanup) state.escapeCleanup();
  state.escapeCleanup = onEscapeKey(document, state.dotNetRef, "OnHoverExit");

  // Re-wire hover intent to include positioner so hovering the popup keeps it open.
  if (state.hoverCleanup) state.hoverCleanup();
  state.hoverCleanup = onHoverIntent(
    trigger,
    positioner,
    { enterDelay: state.config.enterDelay, exitDelay: state.config.exitDelay },
    state.dotNetRef,
    "OnHoverEnter",
    "OnHoverExit"
  );
}

export function animateAndHide(triggerId, positionerId) {
  const state = instances.get(triggerId);

  const positioner = document.getElementById(positionerId);
  // Animate the popup child inside the positioner.
  const popup = positioner?.firstElementChild;
  if (!popup) {
    state?.dotNetRef?.invokeMethodAsync("OnExitAnimationComplete");
    return;
  }

  const ref = state?.dotNetRef;

  animateExitCancellable(popup, {
    onComplete() {
      hidePortalEntry(popup);
      ref?.invokeMethodAsync("OnExitAnimationComplete");
    },
  });
}

export function dispose(triggerId) {
  const state = instances.get(triggerId);
  if (!state) return;

  if (state.hoverCleanup) state.hoverCleanup();
  if (state.escapeCleanup) state.escapeCleanup();
  if (state.autoUpdateId) stopAutoUpdate(state.autoUpdateId);
  instances.delete(triggerId);
}
