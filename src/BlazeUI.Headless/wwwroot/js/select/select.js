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
  restoreFocus,
  showPortalEntry,
  hidePortalEntry,
} from "/_content/BlazeUI.Headless/blazeui.core.js";

const instances = new Map();

export function show(triggerId, popupId, options, ref) {
  let inst = instances.get(popupId);
  if (inst) {
    // Cancel any pending close from a previous hide so it doesn't
    // undo this reopen after the exit animation/timeout fires.
    if (inst.pendingClose) {
      inst.pendingClose.cancel();
      inst.pendingClose = null;
    }
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
    };
    instances.set(popupId, inst);
  }

  inst.dotNetRef = ref;
  const trigger = document.getElementById(triggerId);
  const popup = document.getElementById(popupId);
  if (!trigger || !popup) return;

  inst.previouslyFocused = document.activeElement;

  // Cancel any running CSS animations from a previous close.
  for (const anim of popup.getAnimations()) anim.cancel();
  popup.removeAttribute("data-closed");
  showPortalEntry(popup);

  // Show in the top layer but keep invisible and transition-free until
  // positioned. position() is async (returns a Promise), so we start
  // auto-update (which calls position immediately) and defer the reveal
  // to the next frame, by which time the microtask has resolved and
  // left/top are set to the correct coordinates.
  popup.style.opacity = "0";
  popup.style.transition = "none";
  popup.setAttribute("popover", "manual");
  showPopover(popup);

  const posOptions = { placement: options.placement, offset: options.offset };
  if (inst.autoUpdateId) stopAutoUpdate(inst.autoUpdateId);
  inst.autoUpdateId = startAutoUpdate(trigger, popup, posOptions);

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

  // Keyboard navigation (arrow keys, Home/End) scoped to [role="option"] items.
  if (inst.keyboardNavCleanup) inst.keyboardNavCleanup();
  inst.keyboardNavCleanup = initKeyboardNav(popup, {
    itemSelector: '[role="option"]:not([aria-disabled="true"])',
    onEscape: () => ref.invokeMethodAsync("OnEscapeKey"),
    onHighlight: (item) => ref.invokeMethodAsync("OnHighlightChange", item.id),
    onSelect: (item) => {
      // Trigger the Blazor onclick handler on the item.
      item.click();
    },
  });

  // Focus the selected item, or the first option.
  requestAnimationFrame(() => {
    const selected = popup.querySelector('[role="option"][data-selected]');
    const first = popup.querySelector('[role="option"]:not([aria-disabled="true"])');
    const target = selected || first;
    if (target) target.focus();
  });

  // Click outside to close. Exclude the trigger so that a second click
  // lets the trigger's toggle handler run without a pointerdown race.
  if (inst.clickOutsideCleanup) inst.clickOutsideCleanup();
  setTimeout(() => {
    inst.clickOutsideCleanup = onClickOutside(popup, ref, "OnClickOutside", trigger);
  }, 0);

  // Escape at document level for robustness.
  if (inst.escapeCleanup) inst.escapeCleanup();
  inst.escapeCleanup = onEscapeKey(document, ref, "OnEscapeKey");
}

export function hide(popupId) {
  const inst = instances.get(popupId);
  const popup = document.getElementById(popupId);
  // Cancel any pending reveal rAF so a rapid open→close doesn't re-open.
  if (inst) inst.pendingRevealId++;
  cleanupListeners(inst);

  if (!popup) {
    inst?.dotNetRef?.invokeMethodAsync("OnExitAnimationComplete");
    return;
  }

  const ref = inst?.dotNetRef;

  // Run cancellable exit animation so a rapid reopen can abort it.
  if (inst) {
    inst.pendingClose = animateExitCancellable(popup, {
      onComplete() {
        inst.pendingClose = null;
        hidePopover(popup);
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

function cleanupListeners(inst) {
  if (!inst) return;
  if (inst.clickOutsideCleanup) { inst.clickOutsideCleanup(); inst.clickOutsideCleanup = null; }
  if (inst.escapeCleanup) { inst.escapeCleanup(); inst.escapeCleanup = null; }
  if (inst.keyboardNavCleanup) { inst.keyboardNavCleanup(); inst.keyboardNavCleanup = null; }
  if (inst.autoUpdateId) { stopAutoUpdate(inst.autoUpdateId); inst.autoUpdateId = null; }
}

// -- Scroll arrow support --

const SCROLL_INTERVAL_MS = 40;
const SCROLL_STEP_PX = 4;
let scrollTimerIdCounter = 0;
const scrollTimers = new Map();

/**
 * Returns true if the popup is scrollable in the given direction.
 */
export function isScrollable(popupId, direction) {
  const popup = document.getElementById(popupId);
  if (!popup) return false;
  if (direction === "up") return popup.scrollTop > 0;
  return popup.scrollTop + popup.clientHeight < popup.scrollHeight;
}

/**
 * Starts auto-scrolling the popup in the given direction. Returns a timer ID
 * that can be passed to stopScrollArrow().
 */
export function startScrollArrow(popupId, direction) {
  const popup = document.getElementById(popupId);
  if (!popup) return -1;

  const id = ++scrollTimerIdCounter;
  const delta = direction === "up" ? -SCROLL_STEP_PX : SCROLL_STEP_PX;
  const intervalId = setInterval(() => {
    popup.scrollTop += delta;
  }, SCROLL_INTERVAL_MS);

  scrollTimers.set(id, intervalId);
  return id;
}

/**
 * Stops a previously started scroll arrow timer.
 */
export function stopScrollArrow(timerId) {
  const intervalId = scrollTimers.get(timerId);
  if (intervalId != null) {
    clearInterval(intervalId);
    scrollTimers.delete(timerId);
  }
}

/**
 * Returns the trimmed text content of a select item element.
 * Used as a fallback label when no explicit Label parameter is set.
 */
export function getItemLabel(itemId) {
  const el = document.getElementById(itemId);
  return el ? el.textContent.trim() : null;
}

export function dispose(popupId) {
  const inst = instances.get(popupId);
  if (!inst) return;
  if (inst.pendingClose) {
    inst.pendingClose.cancel();
    inst.pendingClose = null;
  }
  cleanupListeners(inst);
  inst.previouslyFocused = null;
  inst.dotNetRef = null;
  instances.delete(popupId);

  // Clean up any lingering scroll arrow timers.
  for (const [id, intervalId] of scrollTimers) {
    clearInterval(intervalId);
    scrollTimers.delete(id);
  }
}
