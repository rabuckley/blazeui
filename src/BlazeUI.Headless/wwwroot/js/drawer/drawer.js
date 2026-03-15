// Drawer JS — manages a <div role="dialog"> overlay (not native <dialog>).
// Visibility is controlled via CSS (data-open/data-closed attributes + hidden).
// Focus trapping, Escape key, and backdrop click are handled manually.
// Swipe-to-dismiss uses continuous pointer tracking with CSS variable feedback.
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

// Module-level body scroll lock with reference counting. There is only one
// body element, so we track how many drawers currently hold the lock.
let bodyLockCount = 0;
let savedBodyStyles = null;

// Vaul-style body scroll lock: freeze the body in place and block
// pointer events so only the drawer content (with pointer-events: auto)
// is interactive.
function lockBody() {
  bodyLockCount++;
  if (bodyLockCount > 1) return;

  const body = document.body;
  savedBodyStyles = {
    overflow: body.style.overflow,
    overscrollBehavior: body.style.overscrollBehavior,
    pointerEvents: body.style.pointerEvents,
    position: body.style.position,
    top: body.style.top,
    left: body.style.left,
    right: body.style.right,
    bottom: body.style.bottom,
  };
  const scrollY = window.scrollY;
  body.style.overflow = "hidden";
  body.style.overscrollBehavior = "contain";
  body.style.pointerEvents = "none";
  body.style.position = "relative";
  body.style.top = "0px";
  body.style.left = "0px";
  body.style.right = "0px";
  body.style.bottom = "0px";
}

function unlockBody() {
  if (bodyLockCount <= 0) return;
  bodyLockCount--;
  if (bodyLockCount > 0) return;

  if (!savedBodyStyles) return;
  const body = document.body;
  Object.assign(body.style, savedBodyStyles);
  savedBodyStyles = null;
}

// -- Swipe-to-dismiss --
// Vaul-style drag tracking: the drawer follows the user's finger during a
// swipe gesture and either dismisses or snaps back on release.

// Distance in px the user must drag before the gesture becomes a swipe.
// Prevents accidental dismissal from small taps/scrolls.
const DRAG_THRESHOLD = 8;

// After the threshold, the drawer dismisses if the user drags further than
// this distance OR exceeds the velocity threshold.
const DISMISS_DISTANCE = 100;
const DISMISS_VELOCITY = 400; // px/s

function getSwipeDirection(popup) {
  return popup.getAttribute("data-swipe-direction") || "down";
}

function isHorizontalDirection(dir) {
  return dir === "left" || dir === "right";
}

// Compute how many pixels the pointer has moved in the dismiss direction.
// Returns 0 when the pointer moves opposite to dismiss (drawer can't be
// pulled further in than its resting position).
function dismissDelta(dx, dy, direction) {
  switch (direction) {
    case "down":  return Math.max(0, dy);
    case "up":    return Math.max(0, -dy);
    case "right": return Math.max(0, dx);
    case "left":  return Math.max(0, -dx);
  }
  return 0;
}

// Convert a dismiss delta into a CSS translate for the popup.
function translateForDelta(delta, direction) {
  const h = isHorizontalDirection(direction);
  const sign = direction === "up" || direction === "left" ? -1 : 1;
  const px = sign * delta;
  return h ? `translateX(${px}px)` : `translateY(${px}px)`;
}

// Attach swipe-to-dismiss pointer listeners to an open drawer popup.
// Returns a cleanup function that removes all listeners.
function attachDragToDismiss(popup, ref) {
  const direction = getSwipeDirection(popup);
  const horiz = isHorizontalDirection(direction);

  let startX = 0, startY = 0, startTime = 0;
  let isDragging = false;
  let thresholdPassed = false;

  const overlay = popup.previousElementSibling;

  const onPointerDown = (e) => {
    if (e.pointerType === "mouse" && e.button !== 0) return;

    // If the touch starts inside a scrollable child that still has room to
    // scroll in the dismiss direction, let the browser handle it as a scroll.
    if (canScrollInDismissDirection(e.target, popup, direction)) return;

    isDragging = true;
    thresholdPassed = false;
    startX = e.clientX;
    startY = e.clientY;
    startTime = Date.now();
    popup.setPointerCapture(e.pointerId);
  };

  const onPointerMove = (e) => {
    if (!isDragging) return;

    const dx = e.clientX - startX;
    const dy = e.clientY - startY;
    const delta = dismissDelta(dx, dy, direction);

    // Ignore movement until the user drags past the threshold. This avoids
    // hijacking small scroll gestures.
    if (!thresholdPassed) {
      if (delta < DRAG_THRESHOLD) return;
      thresholdPassed = true;
    }

    // Visual feedback: translate the drawer and set CSS variables so
    // styled templates can react (e.g. backdrop opacity).
    popup.setAttribute("data-swiping", "");
    popup.style.transitionProperty = "none";
    popup.style.transform = translateForDelta(delta, direction);
    popup.style.setProperty("--drawer-drag", `${delta}px`);

    const progress = Math.min(delta / DISMISS_DISTANCE, 1);
    popup.style.setProperty("--drawer-snap-progress", String(progress));

    // Fade backdrop proportionally.
    if (overlay) {
      overlay.style.opacity = String(1 - progress * 0.6);
    }
  };

  const onPointerUp = (e) => {
    if (!isDragging) return;
    isDragging = false;

    if (!thresholdPassed) return;

    const dx = e.clientX - startX;
    const dy = e.clientY - startY;
    const dt = (Date.now() - startTime) || 1;
    const delta = dismissDelta(dx, dy, direction);
    const velocity = (delta / dt) * 1000; // px/s

    // Clear drag state before deciding dismiss vs snap-back.
    popup.removeAttribute("data-swiping");
    popup.style.removeProperty("--drawer-drag");
    popup.style.removeProperty("--drawer-snap-progress");

    if (delta >= DISMISS_DISTANCE || velocity >= DISMISS_VELOCITY) {
      // Dismiss — let the Blazor close cycle handle cleanup.
      // First, slide the drawer fully off-screen for immediate feedback.
      const offscreen = horiz ? popup.offsetWidth : popup.offsetHeight;
      popup.style.transitionProperty = "transform";
      popup.style.transitionDuration = "0.2s";
      popup.style.transitionTimingFunction = "cubic-bezier(0.32, 0.72, 0, 1)";
      popup.style.transform = translateForDelta(offscreen, direction);
      if (overlay) {
        overlay.style.transition = "opacity 0.2s";
        overlay.style.opacity = "0";
      }
      ref.invokeMethodAsync("OnEscapeKey").catch(() => {});
    } else {
      // Snap back to resting position.
      popup.style.transitionProperty = "transform";
      popup.style.transitionDuration = "0.5s";
      popup.style.transitionTimingFunction = "cubic-bezier(0.32, 0.72, 0, 1)";
      popup.style.transform = "";
      if (overlay) {
        overlay.style.transition = "opacity 0.5s cubic-bezier(0.32, 0.72, 0, 1)";
        overlay.style.opacity = "";
      }
    }
  };

  const onPointerCancel = () => {
    if (!isDragging) return;
    isDragging = false;
    popup.removeAttribute("data-swiping");
    popup.style.removeProperty("--drawer-drag");
    popup.style.removeProperty("--drawer-snap-progress");
    popup.style.transitionProperty = "transform";
    popup.style.transform = "";
    if (overlay) {
      overlay.style.transition = "";
      overlay.style.opacity = "";
    }
  };

  popup.addEventListener("pointerdown", onPointerDown);
  popup.addEventListener("pointermove", onPointerMove);
  popup.addEventListener("pointerup", onPointerUp);
  popup.addEventListener("pointercancel", onPointerCancel);

  return () => {
    popup.removeEventListener("pointerdown", onPointerDown);
    popup.removeEventListener("pointermove", onPointerMove);
    popup.removeEventListener("pointerup", onPointerUp);
    popup.removeEventListener("pointercancel", onPointerCancel);
  };
}

// Check whether an element (or its scrollable ancestor up to `boundary`) can
// scroll further in the drawer's dismiss direction. When true, the browser's
// native scroll should take priority over the drag-to-dismiss gesture.
function canScrollInDismissDirection(target, boundary, direction) {
  let el = target;
  while (el && el !== boundary) {
    const style = getComputedStyle(el);
    const isScrollable =
      style.overflowY === "auto" || style.overflowY === "scroll" ||
      style.overflowX === "auto" || style.overflowX === "scroll";

    if (isScrollable) {
      switch (direction) {
        case "down":  return el.scrollTop < el.scrollHeight - el.clientHeight;
        case "up":    return el.scrollTop > 0;
        case "right": return el.scrollLeft < el.scrollWidth - el.clientWidth;
        case "left":  return el.scrollLeft > 0;
      }
    }
    el = el.parentElement;
  }
  return false;
}

export function show(popupId, ref) {
  let inst = instances.get(popupId);
  if (inst) {
    // Cancel any in-progress close animation so a rapid reopen works.
    if (inst.pendingClose) { inst.pendingClose.cancel(); inst.pendingClose = null; }
    // Cancel any running CSS animations from a previous close.
    const prevPopup = document.getElementById(popupId);
    if (prevPopup) {
      for (const anim of prevPopup.getAnimations()) anim.cancel();
      prevPopup.removeAttribute("data-closed");
      prevPopup.style.display = "";
    }
    if (inst.escapeCleanup) { inst.escapeCleanup(); }
    if (inst.focusTrapCleanup) { inst.focusTrapCleanup(); }
    if (inst.backdropClickCleanup) { inst.backdropClickCleanup(); }
    if (inst.dragCleanup) { inst.dragCleanup(); }
  }

  inst = {
    escapeCleanup: null,
    focusTrapCleanup: null,
    backdropClickCleanup: null,
    dragCleanup: null,
    pendingClose: null,
    previouslyFocused: null,
    dotNetRef: ref,
  };
  instances.set(popupId, inst);

  const popup = document.getElementById(popupId);
  if (!popup) return;

  // Clear display:none set by a previous hide()'s post-animation cleanup.
  popup.style.display = "";
  showPortalEntry(popup);
  const backdrop = popup.previousElementSibling;
  if (backdrop) backdrop.style.display = "";

  inst.previouslyFocused = document.activeElement;
  lockBody();

  // Make visible — set inline pointer-events so content is interactive
  // while the body scroll-lock blocks everything else. Also set vaul-style
  // interaction/transition properties matching vaul's constants:
  // TRANSITIONS.DURATION = 0.5, TRANSITIONS.EASE = [0.32, 0.72, 0, 1]
  popup.style.pointerEvents = "auto";
  popup.style.touchAction = "none";
  popup.style.userSelect = "none";
  popup.style.willChange = "transform";
  popup.style.transitionProperty = "transform";
  popup.style.transitionDuration = "0.5s";
  popup.style.transitionTimingFunction = "cubic-bezier(0.32, 0.72, 0, 1)";
  popup.removeAttribute("hidden");

  // JS owns data-open/data-closed — Blazor does not render these attributes
  // to avoid re-render cycles resetting animation state.
  popup.setAttribute("data-open", "");
  animateEntry(popup);

  // Attach drag-to-dismiss so the user can swipe the open drawer away.
  inst.dragCleanup = attachDragToDismiss(popup, ref);

  // Focus trap inside the popup.
  inst.focusTrapCleanup = trapFocus(popup);

  // Focus the first tabbable element.
  requestAnimationFrame(() => {
    const first = popup.querySelector(
      'a[href], button:not([disabled]), input:not([disabled]), [tabindex]:not([tabindex="-1"])'
    );
    if (first) first.focus();
  });

  // Escape to close — document-level handler since focus may still be on the
  // trigger when Playwright sends the keypress (see CLAUDE.md).
  inst.escapeCleanup = onEscapeKey(document, ref, "OnEscapeKey");

  // Backdrop click — listen directly on the overlay sibling rather than using
  // onClickOutside because the overlay needs explicit pointer-events: auto
  // (body scroll lock sets pointer-events: none on the body).
  const overlay = popup.previousElementSibling;
  if (overlay) {
    overlay.style.pointerEvents = "auto";
    const onOverlayClick = () => {
      ref.invokeMethodAsync("OnEscapeKey");
    };
    overlay.addEventListener("click", onOverlayClick);
    inst.backdropClickCleanup = () => {
      overlay.removeEventListener("click", onOverlayClick);
      overlay.style.pointerEvents = "";
    };
  }
}

export function hide(popupId) {
  const inst = instances.get(popupId);
  const popup = document.getElementById(popupId);

  if (inst) {
    if (inst.escapeCleanup) { inst.escapeCleanup(); inst.escapeCleanup = null; }
    if (inst.focusTrapCleanup) { inst.focusTrapCleanup(); inst.focusTrapCleanup = null; }
    if (inst.backdropClickCleanup) { inst.backdropClickCleanup(); inst.backdropClickCleanup = null; }
    if (inst.dragCleanup) { inst.dragCleanup(); inst.dragCleanup = null; }
  }

  if (!popup) {
    unlockBody();
    inst?.dotNetRef?.invokeMethodAsync("OnExitAnimationComplete");
    return;
  }

  const ref = inst?.dotNetRef;

  // Clear interaction styles but keep transition properties for the exit
  // animation. The CSS keyframe animation (animate-vaul-slide-to-right etc.)
  // is triggered by data-closed which animateExitCancellable sets.
  popup.style.pointerEvents = "";
  popup.style.touchAction = "";
  popup.style.userSelect = "";
  popup.style.willChange = "";
  popup.style.transitionProperty = "";
  popup.style.transitionDuration = "";
  popup.style.transitionTimingFunction = "";
  popup.style.transform = "";

  // Clean up any lingering backdrop inline styles from drag gestures.
  const overlay = popup.previousElementSibling;
  if (overlay) {
    overlay.style.transition = "";
    overlay.style.opacity = "";
  }

  // Run cancellable exit animation so a rapid reopen can abort it.
  // animateExitCancellable sets data-closed, waits for the CSS keyframe
  // animation to finish, then calls onComplete.
  if (inst) {
    inst.pendingClose = animateExitCancellable(popup, {
      onComplete() {
        inst.pendingClose = null;
        unlockBody();
        // Hide to prevent post-animation flash during Blazor roundtrip.
        popup.style.display = "none";
        if (overlay) overlay.style.display = "none";
        hidePortalEntry(popup);
        ref?.invokeMethodAsync("OnExitAnimationComplete");
      },
    });
  } else {
    unlockBody();
  }

  if (inst?.previouslyFocused) {
    restoreFocus(inst.previouslyFocused);
    inst.previouslyFocused = null;
  }
}

// -- Swipe area support --
// Tracks pointer events on a swipe zone to open the drawer via gesture.

const SWIPE_OPEN_VELOCITY = 400; // px/s
const SWIPE_OPEN_DISTANCE = 40;  // px
const swipeAreas = new Map();

/**
 * Initializes a swipe area that listens for pointer drag gestures to open the drawer.
 * @param {string} swipeAreaId - ID of the swipe area element
 * @param {string} popupId - ID of the drawer popup
 * @param {string} direction - swipe direction to open: 'top' | 'bottom' | 'left' | 'right'
 * @param {object} ref - DotNetObjectReference for callbacks
 */
export function initSwipeArea(swipeAreaId, popupId, direction, ref) {
  const el = document.getElementById(swipeAreaId);
  if (!el) return;

  let startX = 0, startY = 0, startTime = 0;
  let isDragging = false;

  const horiz = isHorizontalDirection(direction);

  const onPointerDown = (e) => {
    if (e.pointerType === "mouse" && e.button !== 0) return;
    isDragging = true;
    startX = e.clientX;
    startY = e.clientY;
    startTime = Date.now();
    el.setPointerCapture(e.pointerId);
    el.setAttribute("data-swiping", "");
  };

  const onPointerMove = (e) => {
    if (!isDragging) return;

    const dx = e.clientX - startX;
    const dy = e.clientY - startY;

    // Compute how far the user has swiped in the open direction.
    let openDelta = 0;
    switch (direction) {
      case "left":   openDelta = Math.max(0, dx); break;
      case "right":  openDelta = Math.max(0, -dx); break;
      case "top":    openDelta = Math.max(0, dy); break;
      case "bottom": openDelta = Math.max(0, -dy); break;
    }

    const progress = Math.min(openDelta / SWIPE_OPEN_DISTANCE, 1);
    el.style.setProperty("--swipe-open-progress", String(progress));
  };

  const onPointerUp = (e) => {
    if (!isDragging) return;
    isDragging = false;
    el.removeAttribute("data-swiping");
    el.style.removeProperty("--swipe-open-progress");

    const dx = e.clientX - startX;
    const dy = e.clientY - startY;
    const dt = (Date.now() - startTime) || 1;
    const delta = horiz ? dx : dy;
    const velocity = (Math.abs(delta) / dt) * 1000; // px/s

    // Determine if the swipe is in the open direction.
    let isOpenSwipe = false;
    switch (direction) {
      case "left":   isOpenSwipe = delta > 0; break;
      case "right":  isOpenSwipe = delta < 0; break;
      case "top":    isOpenSwipe = delta > 0; break;
      case "bottom": isOpenSwipe = delta < 0; break;
    }

    if (isOpenSwipe && (Math.abs(delta) > SWIPE_OPEN_DISTANCE || velocity > SWIPE_OPEN_VELOCITY)) {
      // OnEscapeKey toggles open — we reuse the same callback since the root
      // just needs SetOpen(true/false).
      ref.invokeMethodAsync("OnEscapeKey").catch(() => {});
    }
  };

  const onPointerCancel = () => {
    isDragging = false;
    el.removeAttribute("data-swiping");
    el.style.removeProperty("--swipe-open-progress");
  };

  el.addEventListener("pointerdown", onPointerDown);
  el.addEventListener("pointermove", onPointerMove);
  el.addEventListener("pointerup", onPointerUp);
  el.addEventListener("pointercancel", onPointerCancel);

  swipeAreas.set(swipeAreaId, () => {
    el.removeEventListener("pointerdown", onPointerDown);
    el.removeEventListener("pointermove", onPointerMove);
    el.removeEventListener("pointerup", onPointerUp);
    el.removeEventListener("pointercancel", onPointerCancel);
  });
}

export function dispose(popupId) {
  const inst = instances.get(popupId);
  if (!inst) return;

  if (inst.pendingClose) { inst.pendingClose.cancel(); }
  if (inst.escapeCleanup) { inst.escapeCleanup(); }
  if (inst.focusTrapCleanup) { inst.focusTrapCleanup(); }
  if (inst.backdropClickCleanup) { inst.backdropClickCleanup(); }
  if (inst.dragCleanup) { inst.dragCleanup(); }

  unlockBody();
  instances.delete(popupId);

  // Clean up any swipe areas associated with this drawer.
  for (const [id, cleanup] of swipeAreas) {
    cleanup();
    swipeAreas.delete(id);
  }
}
