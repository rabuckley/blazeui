import {
  onClickOutside,
  onEscapeKey,
  trapFocus,
  position,
  startAutoUpdate,
  stopAutoUpdate,
  animateEntry,
  animateExitCancellable,
  showPopover,
  hidePopover,
  showPortalEntry,
  hidePortalEntry,
} from "/_content/BlazeUI.Headless/blazeui.core.js";

const instances = new Map();

function cancelPendingClose(inst) {
  if (inst.pendingClose) {
    inst.pendingClose.cancel();
    inst.pendingClose = null;
  }
}

export function show(triggerId, popupId, options, ref) {
  let inst = instances.get(popupId);
  if (inst) {
    cancelPendingClose(inst);
  } else {
    inst = {
      clickOutsideCleanup: null,
      escapeCleanup: null,
      focusTrapCleanup: null,
      autoUpdateId: null,
      dotNetRef: null,
      pendingClose: null,
      pendingRevealId: 0,
    };
    instances.set(popupId, inst);
  }

  inst.dotNetRef = ref;
  const trigger = document.getElementById(triggerId);
  const popup = document.getElementById(popupId);
  if (!trigger || !popup) return;

  // Cancel any running CSS animations from a previous close so the popup
  // doesn't blend the old exit animation with the new entry animation.
  for (const anim of popup.getAnimations()) anim.cancel();

  // Reset close state in case we're reopening during a close animation.
  popup.removeAttribute("data-closed");
  // Clear display:none set by hide()'s post-animation cleanup.
  popup.style.display = "";
  showPortalEntry(popup);

  // Show in the top layer but keep invisible until positioned. Without this,
  // the first open animates from (0,0) because position() is async and the
  // CSS entry animation starts before floating-ui computes the correct coords.
  popup.style.opacity = "0";
  popup.style.transition = "none";
  popup.setAttribute("popover", "manual");
  showPopover(popup);

  if (inst.autoUpdateId) stopAutoUpdate(inst.autoUpdateId);
  inst.autoUpdateId = startAutoUpdate(trigger, popup, options);

  // Defer reveal to next frame — by then the async position() has resolved.
  // Track the reveal so hide() can cancel it if called before the rAF fires.
  const revealId = ++inst.pendingRevealId;
  requestAnimationFrame(() => {
    if (inst.pendingRevealId !== revealId) return; // cancelled by hide()
    popup.offsetHeight;
    popup.style.transition = "";
    popup.style.opacity = "";
    popup.setAttribute("data-open", "");
    animateEntry(popup);

    // Focus the first tabbable element inside the popup.
    requestAnimationFrame(() => {
      const first = popup.querySelector(
        'a[href], button:not([disabled]), input:not([disabled]), [tabindex]:not([tabindex="-1"])'
      );
      if (first) first.focus();
    });
  });

  // Focus trap.
  if (inst.focusTrapCleanup) inst.focusTrapCleanup();
  inst.focusTrapCleanup = trapFocus(popup);

  // Click outside to close — excludes the trigger because its own onclick
  // handles toggling. Without this, pointerdown fires click-outside (close)
  // before click fires toggle (reopen), causing a race.
  if (inst.clickOutsideCleanup) inst.clickOutsideCleanup();
  // Delay to avoid the opening click from immediately closing.
  setTimeout(() => {
    const handler = (e) => {
      if (!popup.contains(e.target) && !trigger.contains(e.target)) {
        ref.invokeMethodAsync("OnClickOutside");
      }
    };
    document.addEventListener("pointerdown", handler);
    inst.clickOutsideCleanup = () => document.removeEventListener("pointerdown", handler);
  }, 0);

  // Escape to close.
  if (inst.escapeCleanup) inst.escapeCleanup();
  inst.escapeCleanup = onEscapeKey(document, ref, "OnEscapeKey");
}

export function hide(popupId) {
  const inst = instances.get(popupId);
  // Cancel any pending reveal rAF so a rapid open→close doesn't re-open.
  if (inst) inst.pendingRevealId++;
  const popup = document.getElementById(popupId);
  cleanupListeners(inst);

  if (!popup) {
    inst?.dotNetRef?.invokeMethodAsync("OnExitAnimationComplete");
    return;
  }

  const ref = inst?.dotNetRef;

  // Run cancellable exit animation — sets data-closed, listens animationend,
  // then removes from top-layer and notifies Blazor when complete.
  if (inst) {
    inst.pendingClose = animateExitCancellable(popup, {
      onComplete() {
        inst.pendingClose = null;
        hidePopover(popup);
        // Explicitly set display:none — the Popover API's [popover]:not(:popover-open)
        // selector has lower specificity than Tailwind's layout classes (e.g. flex).
        popup.style.display = "none";
        hidePortalEntry(popup);
        ref?.invokeMethodAsync("OnExitAnimationComplete");
      },
    });
  }
}

function cleanupListeners(inst) {
  if (!inst) return;
  if (inst.clickOutsideCleanup) { inst.clickOutsideCleanup(); inst.clickOutsideCleanup = null; }
  if (inst.escapeCleanup) { inst.escapeCleanup(); inst.escapeCleanup = null; }
  if (inst.focusTrapCleanup) { inst.focusTrapCleanup(); inst.focusTrapCleanup = null; }
  if (inst.autoUpdateId) { stopAutoUpdate(inst.autoUpdateId); inst.autoUpdateId = null; }
}

export function dispose(popupId) {
  const inst = instances.get(popupId);
  if (!inst) return;
  cancelPendingClose(inst);
  cleanupListeners(inst);
  inst.dotNetRef = null;
  instances.delete(popupId);
}
