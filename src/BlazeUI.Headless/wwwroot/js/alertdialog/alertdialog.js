// AlertDialog JS — like Dialog but pointer dismissal is disabled.
// Escape still closes the dialog (per ARIA alertdialog spec).
import {
  onEscapeKey,
  trapFocus,
  restoreFocus,
  animateEntry,
  animateExitCancellable,
  showPortalEntry,
  hidePortalEntry,
} from "/_content/BlazeUI.Headless/blazeui.core.js";

const instances = new Map();

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
    if (prevInst.pendingClose) { prevInst.pendingClose.cancel(); prevInst.pendingClose = null; }
    if (prevInst.escapeCleanup) { prevInst.escapeCleanup(); }
    if (prevInst.focusTrapCleanup) { prevInst.focusTrapCleanup(); }
  }

  const inst = {
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
  dialog.style.display = "";
  showPortalEntry(dialog);
  const backdrop = dialog.previousElementSibling;
  if (backdrop?.getAttribute("role") === "presentation") {
    backdrop.style.display = "";
    animateEntry(backdrop);
  }

  inst.previouslyFocused = document.activeElement;

  lockBodyScroll();

  // JS owns data-open/data-closed — Blazor does not render these attributes
  // to avoid re-render cycles resetting animation state.
  dialog.setAttribute("data-open", "");
  animateEntry(dialog);

  inst.focusTrapCleanup = trapFocus(dialog);

  requestAnimationFrame(() => {
    const first = dialog.querySelector(
      'a[href], button:not([disabled]), input:not([disabled]), [tabindex]:not([tabindex="-1"])'
    );
    if (first) first.focus();
  });

  inst.escapeCleanup = onEscapeKey(document, ref, "OnEscapeKey");
}

export function hide(popupId) {
  const inst = instances.get(popupId);
  const dialog = document.getElementById(popupId);

  if (inst) {
    if (inst.escapeCleanup) { inst.escapeCleanup(); inst.escapeCleanup = null; }
    if (inst.focusTrapCleanup) { inst.focusTrapCleanup(); inst.focusTrapCleanup = null; }
  }

  if (!dialog) {
    if (inst) unlockBodyScroll();
    inst?.dotNetRef?.invokeMethodAsync("OnExitAnimationComplete");
    return;
  }

  const ref = inst?.dotNetRef;

  if (inst) {
    inst.pendingClose = animateExitCancellable(dialog, {
      onComplete() {
        inst.pendingClose = null;
        unlockBodyScroll();
        dialog.style.display = "none";
        const backdrop = dialog.previousElementSibling;
        if (backdrop?.getAttribute("role") === "presentation") {
          backdrop.style.display = "none";
        }
        hidePortalEntry(dialog);
        ref?.invokeMethodAsync("OnExitAnimationComplete");
      },
    });
  }

  if (inst?.previouslyFocused) {
    restoreFocus(inst.previouslyFocused);
    inst.previouslyFocused = null;
  }
}

export function dispose(popupId) {
  const inst = instances.get(popupId);
  if (!inst) return;

  if (inst.pendingClose) { inst.pendingClose.cancel(); }
  if (inst.escapeCleanup) { inst.escapeCleanup(); }
  if (inst.focusTrapCleanup) { inst.focusTrapCleanup(); }

  instances.delete(popupId);
}
