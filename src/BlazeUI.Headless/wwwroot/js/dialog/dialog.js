import {
  onClickOutside,
  onEscapeKey,
  trapFocus,
  restoreFocus,
  animateEntry,
  animateExitCancellable,
  showPortalEntry,
  hidePortalEntry,
} from "/_content/BlazeUI.Headless/blazeui.core.js";

const instances = new Map();

// Body scroll lock — reference counting for nested dialogs.
let scrollLockCount = 0;
let savedBodyOverflow = null;

function lockBodyScroll() {
  if (scrollLockCount === 0) {
    savedBodyOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
  }
  scrollLockCount++;
}

function unlockBodyScroll() {
  scrollLockCount--;
  if (scrollLockCount <= 0) {
    document.body.style.overflow = savedBodyOverflow ?? "";
    scrollLockCount = 0;
    savedBodyOverflow = null;
  }
}

export function show(popupId, ref) {
  const prevInst = instances.get(popupId);
  if (prevInst) {
    // Clean up previous instance state before reinitialising.
    if (prevInst.pendingClose) { prevInst.pendingClose.cancel(); prevInst.pendingClose = null; }
    if (prevInst.escapeCleanup) { prevInst.escapeCleanup(); }
    if (prevInst.focusTrapCleanup) { prevInst.focusTrapCleanup(); }
  }

  const inst = {
    clickOutsideCleanup: null,
    escapeCleanup: null,
    focusTrapCleanup: null,
    previouslyFocused: null,
    pendingClose: null,
    dotNetRef: ref,
  };
  instances.set(popupId, inst);

  const dialog = document.getElementById(popupId);
  if (!dialog) return;

  // Cancel running CSS animations on reopen so the entry animation starts clean.
  if (prevInst) {
    for (const anim of dialog.getAnimations()) anim.cancel();
  }
  dialog.removeAttribute("data-closed");
  // Clear display:none set by hide()'s post-animation cleanup.
  dialog.style.display = "";
  showPortalEntry(dialog);
  const backdrop = dialog.previousElementSibling;
  if (backdrop?.getAttribute("role") === "presentation") {
    backdrop.style.display = "";
    animateEntry(backdrop);
  }

  // Remember what was focused before opening to restore on close.
  inst.previouslyFocused = document.activeElement;

  lockBodyScroll();

  // JS owns data-open/data-closed — Blazor does not render these attributes
  // to avoid re-render cycles resetting animation state.
  dialog.setAttribute("data-open", "");
  animateEntry(dialog);

  // Focus trap inside the dialog.
  inst.focusTrapCleanup = trapFocus(dialog);

  // Focus the first tabbable element.
  requestAnimationFrame(() => {
    const first = dialog.querySelector(
      'a[href], button:not([disabled]), input:not([disabled]), [tabindex]:not([tabindex="-1"])'
    );
    if (first) first.focus();
  });

  // Escape to close — document-level handler since there's no native cancel event.
  inst.escapeCleanup = onEscapeKey(document, ref, "OnEscapeKey");

  // Click outside to close (backdrop click).
  if (inst.clickOutsideCleanup) inst.clickOutsideCleanup();
  setTimeout(() => {
    inst.clickOutsideCleanup = onClickOutside(dialog, ref, "OnClickOutside");
  }, 0);
}

export function hide(popupId) {
  const inst = instances.get(popupId);
  const dialog = document.getElementById(popupId);

  if (inst) {
    if (inst.clickOutsideCleanup) { inst.clickOutsideCleanup(); inst.clickOutsideCleanup = null; }
    if (inst.escapeCleanup) { inst.escapeCleanup(); inst.escapeCleanup = null; }
    if (inst.focusTrapCleanup) { inst.focusTrapCleanup(); inst.focusTrapCleanup = null; }
  }

  if (!dialog) {
    if (inst) unlockBodyScroll();
    inst?.dotNetRef?.invokeMethodAsync("OnExitAnimationComplete");
    return;
  }

  const ref = inst?.dotNetRef;

  // Run cancellable exit animation so a rapid reopen can abort it.
  if (inst) {
    inst.pendingClose = animateExitCancellable(dialog, {
      onComplete() {
        inst.pendingClose = null;
        unlockBodyScroll();
        // Hide to prevent post-animation flash during Blazor roundtrip.
        dialog.style.display = "none";
        // Also hide the backdrop sibling (rendered immediately before the popup
        // in the DialogContent template) to prevent it lingering visibly.
        const backdrop = dialog.previousElementSibling;
        if (backdrop?.getAttribute("role") === "presentation") {
          backdrop.style.display = "none";
        }
        hidePortalEntry(dialog);
        ref?.invokeMethodAsync("OnExitAnimationComplete");
      },
    });
  }

  // Restore focus to the previously focused element.
  if (inst?.previouslyFocused) {
    restoreFocus(inst.previouslyFocused);
    inst.previouslyFocused = null;
  }
}

export function dispose(popupId) {
  const inst = instances.get(popupId);
  if (!inst) return;

  if (inst.pendingClose) { inst.pendingClose.cancel(); }
  if (inst.clickOutsideCleanup) { inst.clickOutsideCleanup(); }
  if (inst.escapeCleanup) { inst.escapeCleanup(); }
  if (inst.focusTrapCleanup) { inst.focusTrapCleanup(); }

  instances.delete(popupId);
}
