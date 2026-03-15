import {
  onClickOutside,
  onEscapeKey,
  initKeyboardNav,
  position,
  startAutoUpdate,
  stopAutoUpdate,
  animateEntry,
  animateExitCancellable,
  showPopover,
  hidePopover,
  showPortalEntry,
  hidePortalEntry,
  restoreFocus,
} from "/_content/BlazeUI.Headless/blazeui.core.js";

const instances = new Map();

// Track the click event's `detail` property to distinguish keyboard from
// mouse opening, mirroring Base UI's useEnhancedClickHandler approach.
// detail === 0 means the click was synthesised from Enter/Space on a button;
// detail >= 1 means a real pointer click.
let lastTriggerClickDetail = -1;
document.addEventListener("click", (e) => {
  if (e.target.closest('[aria-haspopup="menu"]')) {
    lastTriggerClickDetail = e.detail;
  }
}, true); // capture phase — fires before Blazor's bubble-phase handler

// Tabbable element helpers for focus guards (mirrors Base UI's
// getTabbableBeforeElement / getTabbableAfterElement).
const TABBABLE =
  'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), ' +
  'textarea:not([disabled]), [tabindex]:not([tabindex="-1"]):not([popover])';

function getTabbablesInDocument() {
  return [...document.querySelectorAll(TABBABLE)]
    .filter(el => el.offsetParent !== null && !el.closest("[popover]"));
}

function getTabbableAfter(ref) {
  const all = getTabbablesInDocument();
  const idx = all.indexOf(ref);
  return idx >= 0 && idx < all.length - 1 ? all[idx + 1] : null;
}

function getTabbableBefore(ref) {
  const all = getTabbablesInDocument();
  const idx = all.indexOf(ref);
  return idx > 0 ? all[idx - 1] : null;
}

// Body scroll lock uses module-level reference counting so multiple
// context menus don't clobber each other's overflow state.
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
      keyboardNavCleanup: null,
      autoUpdateId: null,
      dotNetRef: null,
      previouslyFocused: null,
      pendingClose: null,
      pendingRevealId: 0,
      bodyOverflowLocked: false,
    };
    instances.set(popupId, inst);
  }

  inst.dotNetRef = ref;
  const trigger = document.getElementById(triggerId);
  const popup = document.getElementById(popupId);
  if (!trigger || !popup) return;

  inst.previouslyFocused = document.activeElement;

  // Cancel any running CSS animations from a previous close so the popup
  // doesn't blend the old exit animation with the new entry animation.
  for (const anim of popup.getAnimations()) anim.cancel();

  // Reset close state in case we're reopening during a close animation.
  popup.removeAttribute("data-closed");
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
  });

  // Keyboard navigation (arrow keys, typeahead, Home/End).
  // Detect whether this popup is a submenu by checking if the trigger is a menuitem
  // (submenu triggers render role="menuitem"; top-level triggers render role="button").
  const isSubmenu = trigger.getAttribute("role") === "menuitem";

  if (inst.keyboardNavCleanup) inst.keyboardNavCleanup();
  inst.keyboardNavCleanup = initKeyboardNav(popup, {
    onEscape: () => ref.invokeMethodAsync("OnEscapeKey"),

    // ArrowRight on a submenu trigger item → click it to open the nested submenu.
    onSubmenuOpen: (item) => item.click(),

    // ArrowLeft inside a submenu → close it and return focus to the parent trigger.
    onSubmenuClose: isSubmenu
      ? () => { ref.invokeMethodAsync("OnEscapeKey"); trigger.focus(); }
      : undefined,
  });

  // Focus items on pointer hover, matching Base UI's useMenuItemCommonProps
  // onMouseMove behavior. This activates `focus:bg-accent` CSS on the item.
  // Uses mousemove (not pointerenter) so moving within an already-focused
  // item doesn't re-fire, and so the highlight follows the cursor as it
  // moves between items.
  const ITEM_SELECTOR = '[role="menuitem"], [role="menuitemcheckbox"], [role="menuitemradio"]';
  if (inst.hoverFocusCleanup) inst.hoverFocusCleanup();
  const hoverFocusHandler = (e) => {
    const item = e.target.closest(ITEM_SELECTOR);
    if (item && !item.hasAttribute("aria-disabled") && document.activeElement !== item) {
      item.focus();
    }
  };
  popup.addEventListener("mousemove", hoverFocusHandler);
  inst.hoverFocusCleanup = () => popup.removeEventListener("mousemove", hoverFocusHandler);

  // Focus behavior depends on how the menu was opened, mirroring Base UI's
  // useEnhancedClickHandler: event.detail === 0 means keyboard (Enter/Space
  // synthesises a click with detail 0), detail >= 1 means mouse/pointer.
  const openedViaKeyboard = lastTriggerClickDetail === 0;
  lastTriggerClickDetail = -1;
  requestAnimationFrame(() => {
    if (openedViaKeyboard) {
      const first = popup.querySelector('[role="menuitem"]');
      if (first) first.focus();
      else popup.focus();
    } else {
      popup.focus();
    }
  });

  // Tab closes the menu and moves focus to the next/previous tabbable
  // element relative to the trigger. Base UI achieves this with focus-guard
  // sentinel elements, but those don't work inside popover="manual" top-layer
  // elements (the browser skips them and exits the popover directly). Instead
  // we intercept Tab at keydown and programmatically move focus, using the
  // same getTabbableAfter/getTabbableBefore helpers.
  const tabHandler = (e) => {
    if (e.key !== "Tab") return;
    e.preventDefault();
    hide(popupId);
    ref.invokeMethodAsync("OnEscapeKey");
    const target = e.shiftKey
      ? getTabbableBefore(trigger)
      : getTabbableAfter(trigger);
    (target || trigger).focus();
  };
  popup.addEventListener("keydown", tabHandler);
  const prevTabCleanup = inst.tabCleanup;
  inst.tabCleanup = () => popup.removeEventListener("keydown", tabHandler);
  if (prevTabCleanup) prevTabCleanup();

  // Click outside to close. Exclude the trigger so that a second click on
  // it lets the trigger's own toggle handler run without the click-outside
  // handler (which uses pointerdown) closing the menu first.
  if (inst.clickOutsideCleanup) inst.clickOutsideCleanup();
  setTimeout(() => {
    inst.clickOutsideCleanup = onClickOutside(popup, ref, "OnClickOutside", trigger);
  }, 0);

  // Escape to close. For the root popup, register a document-level handler as
  // a fallback (focus might still be on the trigger during SSR→interactive handoff).
  // Submenus don't need this — their popup-level initKeyboardNav handles Escape,
  // and each submenu has its own DotNetRef so OnEscapeKey closes only that level.
  if (inst.escapeCleanup) inst.escapeCleanup();
  if (!isSubmenu) {
    inst.escapeCleanup = onEscapeKey(document, ref, "OnEscapeKey");
  }
}

export function hide(popupId) {
  const inst = instances.get(popupId);
  // Cancel any pending reveal rAF so a rapid open→close doesn't re-open.
  if (inst) inst.pendingRevealId++;
  const popup = document.getElementById(popupId);
  cleanupListeners(inst);
  if (inst?.bodyOverflowLocked) {
    unlockBodyScroll();
    inst.bodyOverflowLocked = false;
  }

  if (!popup) {
    inst?.dotNetRef?.invokeMethodAsync("OnExitAnimationComplete");
    return;
  }

  const ref = inst?.dotNetRef;

  // Run cancellable exit animation — sets data-closed, listens animationend,
  // then removes from top-layer and notifies Blazor when complete.
  // The pending close is tracked so show/showAtPosition can cancel it if the
  // popup is reopened before the close animation finishes.
  if (inst) {
    inst.pendingClose = animateExitCancellable(popup, {
      onComplete() {
        inst.pendingClose = null;
        // For popover-based popups, hidePopover removes from top layer.
        // For non-popover (context menu with positioner), set display:none
        // to prevent post-animation flash during the async Blazor roundtrip.
        if (popup.hasAttribute("popover")) {
          hidePopover(popup);
        } else {
          popup.style.display = "none";
        }
        hidePortalEntry(popup);
        ref?.invokeMethodAsync("OnExitAnimationComplete");
      },
    });
  }

  // Restore focus to trigger.
  if (inst?.previouslyFocused) {
    restoreFocus(inst.previouslyFocused);
    inst.previouslyFocused = null;
  }
}

// Show at an arbitrary position (for ContextMenu virtual anchor).
// When positionerId is provided, position that element instead of the popup
// and avoid the Popover API (the Portal system + z-index handles layering).
export function showAtPosition(popupId, x, y, ref, positionerId) {
  let inst = instances.get(popupId);
  if (inst) {
    cancelPendingClose(inst);
  } else {
    inst = {
      clickOutsideCleanup: null,
      escapeCleanup: null,
      keyboardNavCleanup: null,
      autoUpdateId: null,
      dotNetRef: null,
      previouslyFocused: null,
      pendingClose: null,
      bodyOverflowLocked: false,
    };
    instances.set(popupId, inst);
  }

  inst.dotNetRef = ref;
  const popup = document.getElementById(popupId);
  if (!popup) return;

  const positioner = positionerId ? document.getElementById(positionerId) : null;

  inst.previouslyFocused = document.activeElement;

  // Cancel any running CSS animations from a previous close so the popup
  // doesn't blend the old exit animation with the new entry animation.
  for (const anim of popup.getAnimations()) anim.cancel();
  popup.removeAttribute("data-closed");
  // Clear display:none set by hide()'s post-animation cleanup.
  popup.style.display = "";

  // Position the positioner (or popup if no positioner) at cursor coordinates.
  const target = positioner || popup;

  // Disable transitions while repositioning so `left`/`top` changes snap
  // instead of sliding from the old position.
  target.style.transition = "none";

  Object.assign(target.style, {
    position: "fixed",
    left: `${x}px`,
    top: `${y}px`,
  });
  popup.style.setProperty("--transform-origin", "0 0");

  popup.setAttribute("data-open", "");

  if (!positioner) {
    // Legacy path: no positioner, use Popover API on popup directly.
    popup.setAttribute("popover", "manual");
    showPopover(popup);
  }

  // Force a layout flush so the browser applies the new position with
  // transitions disabled, then restore transitions for the entry animation.
  target.offsetHeight;
  target.style.transition = "";

  animateEntry(popup);

  if (inst.keyboardNavCleanup) inst.keyboardNavCleanup();
  inst.keyboardNavCleanup = initKeyboardNav(popup, {
    onEscape: () => ref.invokeMethodAsync("OnEscapeKey"),
  });

  // Don't auto-focus the first menuitem — context menus open without a
  // highlighted item, matching Base UI. Arrow keys activate items via
  // keyboard nav. Focus the popup container instead for Escape key handling.
  requestAnimationFrame(() => popup.focus());

  if (inst.clickOutsideCleanup) inst.clickOutsideCleanup();
  setTimeout(() => {
    inst.clickOutsideCleanup = onClickOutside(popup, ref, "OnClickOutside");
  }, 0);

  // Lock body scroll to match Base UI's context menu behavior.
  lockBodyScroll();
  inst.bodyOverflowLocked = true;

  if (inst.escapeCleanup) inst.escapeCleanup();
  inst.escapeCleanup = onEscapeKey(document, ref, "OnEscapeKey");
}

function cleanupListeners(inst) {
  if (!inst) return;
  if (inst.clickOutsideCleanup) { inst.clickOutsideCleanup(); inst.clickOutsideCleanup = null; }
  if (inst.escapeCleanup) { inst.escapeCleanup(); inst.escapeCleanup = null; }
  if (inst.keyboardNavCleanup) { inst.keyboardNavCleanup(); inst.keyboardNavCleanup = null; }
  if (inst.tabCleanup) { inst.tabCleanup(); inst.tabCleanup = null; }
  if (inst.hoverFocusCleanup) { inst.hoverFocusCleanup(); inst.hoverFocusCleanup = null; }
  if (inst.autoUpdateId) { stopAutoUpdate(inst.autoUpdateId); inst.autoUpdateId = null; }
}

export function dispose(popupId) {
  const inst = instances.get(popupId);
  if (!inst) return;
  cancelPendingClose(inst);
  cleanupListeners(inst);
  if (inst.bodyOverflowLocked) {
    unlockBodyScroll();
    inst.bodyOverflowLocked = false;
  }
  inst.previouslyFocused = null;
  inst.dotNetRef = null;
  instances.delete(popupId);
}
