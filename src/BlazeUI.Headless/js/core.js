import { computePosition, autoUpdate, flip, shift, offset, size } from "@floating-ui/dom";

// Positioning — wraps @floating-ui/dom

const autoUpdateCleanups = new Map();

export function position(anchorEl, floatingEl, options = {}) {
  const middleware = [
    offset(options.offset ?? 0),
    flip(),
    shift({ padding: options.shiftPadding ?? 5 }),
    size({
      apply({ availableWidth, availableHeight, rects }) {
        // CSS custom properties must use setProperty — Object.assign on
        // CSSStyleDeclaration silently ignores names with leading dashes.
        floatingEl.style.setProperty("--available-width", `${availableWidth}px`);
        floatingEl.style.setProperty("--available-height", `${availableHeight}px`);
        floatingEl.style.setProperty("--anchor-width", `${rects.reference.width}px`);
        floatingEl.style.setProperty("--anchor-height", `${rects.reference.height}px`);
      },
    }),
  ];

  return computePosition(anchorEl, floatingEl, {
    placement: options.placement ?? "bottom",
    middleware,
  }).then(({ x, y, placement }) => {
    Object.assign(floatingEl.style, {
      left: `${x}px`,
      top: `${y}px`,
    });

    // Set transform-origin based on resolved placement for animations.
    const origins = {
      top: "bottom center",
      "top-start": "bottom left",
      "top-end": "bottom right",
      bottom: "top center",
      "bottom-start": "top left",
      "bottom-end": "top right",
      left: "center right",
      "left-start": "top right",
      "left-end": "bottom right",
      right: "center left",
      "right-start": "top left",
      "right-end": "bottom left",
    };
    floatingEl.style.setProperty(
      "--transform-origin",
      origins[placement] ?? "center center"
    );

    return placement;
  });
}

export function startAutoUpdate(anchorEl, floatingEl, options = {}) {
  const id = crypto.randomUUID();
  const cleanup = autoUpdate(anchorEl, floatingEl, () => {
    position(anchorEl, floatingEl, options);
  });
  autoUpdateCleanups.set(id, cleanup);
  return id;
}

export function stopAutoUpdate(id) {
  const cleanup = autoUpdateCleanups.get(id);
  if (cleanup) {
    cleanup();
    autoUpdateCleanups.delete(id);
  }
}

// Focus management

function getTabbableElements(container) {
  const selector = [
    'a[href]',
    'button:not([disabled])',
    'input:not([disabled])',
    'select:not([disabled])',
    'textarea:not([disabled])',
    '[tabindex]:not([tabindex="-1"])',
  ].join(",");
  return [...container.querySelectorAll(selector)].filter(
    (el) => !el.closest("[hidden]") && el.offsetParent !== null
  );
}

export function trapFocus(container) {
  const handler = (e) => {
    if (e.key !== "Tab") return;
    const tabbable = getTabbableElements(container);
    if (tabbable.length === 0) {
      e.preventDefault();
      return;
    }

    const first = tabbable[0];
    const last = tabbable[tabbable.length - 1];

    if (e.shiftKey && document.activeElement === first) {
      e.preventDefault();
      last.focus();
    } else if (!e.shiftKey && document.activeElement === last) {
      e.preventDefault();
      first.focus();
    }
  };

  container.addEventListener("keydown", handler);
  return () => container.removeEventListener("keydown", handler);
}

export function restoreFocus(element) {
  element?.focus();
}

export function setFocus(element) {
  element?.focus();
}

// Animations
// Safety timeout prevents deadlock when no CSS transition is defined (see plan review #1).

export function animateEntry(element) {
  element.setAttribute("data-starting-style", "");
  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      element.removeAttribute("data-starting-style");
    });
  });
}

export function animateExit(element, dotNetRef, callbackMethod, options = {}) {
  const {
    event = "transitionend",
    timeout = 300,
    attribute = "data-ending-style",
    callbackArgs = [],
  } = typeof options === "number" ? { timeout: options } : options;

  const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const effectiveTimeout = reducedMotion ? 0 : timeout;

  element.setAttribute(attribute, "");

  let resolved = false;
  const resolve = () => {
    if (resolved) return;
    resolved = true;
    element.removeAttribute(attribute);
    dotNetRef?.invokeMethodAsync(callbackMethod, ...callbackArgs);
  };

  // Safety timeout fires if the event doesn't (no transition/animation defined, or reduced motion).
  const timer = setTimeout(resolve, effectiveTimeout);

  element.addEventListener(
    event,
    () => {
      clearTimeout(timer);
      resolve();
    },
    { once: true }
  );
}

/// Cancellable exit animation for popups that use CSS animations (not transitions).
/// Sets data-closed, listens for animationend, and calls onComplete when done.
/// Returns { cancel() } — calling cancel() prevents onComplete from firing.
export function animateExitCancellable(element, options = {}) {
  const {
    timeout = 350,
    onComplete = () => {},
  } = options;

  element.removeAttribute("data-open");
  element.setAttribute("data-closed", "");

  const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  let resolved = false;

  const complete = () => {
    if (resolved) return;
    resolved = true;
    onComplete();
  };

  const timer = setTimeout(complete, reducedMotion ? 0 : timeout);
  element.addEventListener("animationend", () => { clearTimeout(timer); complete(); }, { once: true });

  return {
    cancel() {
      clearTimeout(timer);
      resolved = true;
    },
  };
}

// Keyboard navigation

// Orientation-aware keyboard navigation handler factory. Returns a handler with
// signature (e, container) that works with both registerDelegatedHandler and
// direct addEventListener (via initKeyboardNav).
//
// Options:
//   orientation  — "horizontal" | "vertical" | undefined (both directions)
//   loop         — wrap at ends (default true)
//   activateOnFocus — click the item after focusing it (default false)
//   itemSelector — CSS selector for navigable items (default: tabbable elements)
//   focusItem    — (item, allItems) => void (default: item.focus())
//   onSelect     — (item) => void for Enter/Space (default: item.click())
//   onEscape     — () => void for Escape key
//   typeahead    — enable text-content typeahead matching (default false)
//   onSubmenuOpen — (item) => void for cross-axis arrow on [aria-haspopup="menu"]
//   onSubmenuClose — () => void for cross-axis arrow back (closes submenu)
export function createKeyboardNavHandler(options = {}) {
  const {
    orientation,
    loop = true,
    activateOnFocus = false,
    itemSelector,
    focusItem: focusItemFn,
    onSelect,
    onEscape,
    onHighlight,
    typeahead = false,
    onSubmenuOpen,
    onSubmenuClose,
  } = options;

  let typeaheadBuffer = "";
  let typeaheadTimer = null;

  // Determine which arrow keys map to next/prev based on orientation.
  // When orientation is undefined, both horizontal and vertical arrows navigate.
  const isNext = (key) => {
    if (orientation === "horizontal") return key === "ArrowRight";
    if (orientation === "vertical") return key === "ArrowDown";
    return key === "ArrowDown" || key === "ArrowRight";
  };
  const isPrev = (key) => {
    if (orientation === "horizontal") return key === "ArrowLeft";
    if (orientation === "vertical") return key === "ArrowUp";
    return key === "ArrowUp" || key === "ArrowLeft";
  };

  const doFocus = (item, items) => {
    if (focusItemFn) {
      focusItemFn(item, items);
    } else {
      item.focus();
    }
    if (onHighlight) onHighlight(item);
    if (activateOnFocus) item.click();
  };

  const getItems = itemSelector
    ? (container) => [...container.querySelectorAll(itemSelector)]
        .filter((el) => !el.closest("[hidden]") && el.offsetParent !== null)
    : (container) => getTabbableElements(container);

  return (e, container) => {
    const items = getItems(container);
    if (items.length === 0) return;

    const currentIndex = items.indexOf(document.activeElement);

    if (isNext(e.key)) {
      e.preventDefault();
      if (currentIndex === -1) {
        // No item focused (e.g. popup container has focus) — focus first item.
        doFocus(items[0], items);
        return;
      }
      const next = currentIndex < items.length - 1
        ? currentIndex + 1
        : (loop ? 0 : currentIndex);
      doFocus(items[next], items);
      return;
    }

    if (isPrev(e.key)) {
      e.preventDefault();
      if (currentIndex === -1) {
        // No item focused — focus last item.
        doFocus(items[items.length - 1], items);
        return;
      }
      const prev = currentIndex > 0
        ? currentIndex - 1
        : (loop ? items.length - 1 : currentIndex);
      doFocus(items[prev], items);
      return;
    }

    // Cross-axis arrows for submenu navigation:
    // In a vertical menu, ArrowRight on a submenu trigger opens it;
    // ArrowLeft closes the current submenu and returns to the parent.
    if (orientation === "vertical" || orientation === undefined) {
      if (e.key === "ArrowRight" && onSubmenuOpen) {
        const focused = document.activeElement;
        if (focused && focused.getAttribute("aria-haspopup") === "menu") {
          e.preventDefault();
          onSubmenuOpen(focused);
          return;
        }
      }
      if (e.key === "ArrowLeft" && onSubmenuClose) {
        e.preventDefault();
        onSubmenuClose();
        return;
      }
    }

    switch (e.key) {
      case "Home": {
        e.preventDefault();
        doFocus(items[0], items);
        break;
      }
      case "End": {
        e.preventDefault();
        doFocus(items[items.length - 1], items);
        break;
      }
      case "Enter":
      case " ": {
        if (document.activeElement && items.includes(document.activeElement)) {
          e.preventDefault();
          if (onSelect) {
            onSelect(document.activeElement);
          } else {
            document.activeElement.click();
          }
        }
        break;
      }
      case "Escape": {
        if (onEscape) {
          e.preventDefault();
          // Stop propagation so parent/root document-level escape handlers don't
          // also fire. This gives the correct one-level-at-a-time close behavior
          // for nested submenus.
          e.stopPropagation();
          onEscape();
        }
        break;
      }
      default: {
        if (typeahead && e.key.length === 1 && !e.ctrlKey && !e.metaKey) {
          clearTimeout(typeaheadTimer);
          typeaheadBuffer += e.key.toLowerCase();
          typeaheadTimer = setTimeout(() => {
            typeaheadBuffer = "";
          }, 500);

          const match = items.find((item) =>
            item.textContent?.toLowerCase().startsWith(typeaheadBuffer)
          );
          if (match) doFocus(match, items);
        }
        break;
      }
    }
  };
}

export function initKeyboardNav(container, options = {}) {
  const handler = createKeyboardNavHandler({ ...options, typeahead: options.typeahead ?? true });
  const wrappedHandler = (e) => handler(e, container);
  container.addEventListener("keydown", wrappedHandler);
  return () => container.removeEventListener("keydown", wrappedHandler);
}

// Click-outside detection

export function onClickOutside(element, dotNetRef, callbackMethod, excludeElement) {
  const handler = (e) => {
    if (!element.contains(e.target) &&
        (!excludeElement || !excludeElement.contains(e.target))) {
      dotNetRef.invokeMethodAsync(callbackMethod);
    }
  };

  // Use pointerdown so the event fires before focus changes.
  document.addEventListener("pointerdown", handler);
  return () => document.removeEventListener("pointerdown", handler);
}

// Popover API wrappers — top-layer promotion for non-modal overlays.

export function showPopover(element) {
  if (element && typeof element.showPopover === "function") {
    try {
      element.showPopover();
    } catch {
      // Already showing or not supported — ignore.
    }
  }
}

export function hidePopover(element) {
  if (element && typeof element.hidePopover === "function") {
    try {
      element.hidePopover();
    } catch {
      // Already hidden or not supported — ignore.
    }
  }
}

// Portal entry visibility — hides/shows the PortalHost wrapper so closed
// overlay content doesn't leave empty containers in the DOM.

export function hidePortalEntry(popup) {
  const entry = popup?.closest("[data-portal-entry]");
  if (entry) entry.style.display = "none";
}

export function showPortalEntry(popup) {
  const entry = popup?.closest("[data-portal-entry]");
  if (entry) entry.style.display = "contents";
}

// Native <dialog> wrappers — modal overlays with inert + top-layer.

export function showModal(element) {
  if (element && typeof element.showModal === "function" && !element.open) {
    element.showModal();
  }
}

export function closeDialog(element) {
  if (element && typeof element.close === "function" && element.open) {
    element.close();
  }
}

// Hover-intent detection with enter/exit delays (Tooltip, PreviewCard).

export function onHoverIntent(triggerEl, popupEl, options, dotNetRef, enterMethod, exitMethod) {
  const enterDelay = options.enterDelay ?? 300;
  const exitDelay = options.exitDelay ?? 0;
  let enterTimer = null;
  let exitTimer = null;
  let isOpen = false;

  const clearTimers = () => {
    clearTimeout(enterTimer);
    clearTimeout(exitTimer);
  };

  const scheduleOpen = () => {
    clearTimers();
    enterTimer = setTimeout(() => {
      isOpen = true;
      dotNetRef.invokeMethodAsync(enterMethod);
    }, enterDelay);
  };

  const scheduleClose = () => {
    clearTimers();
    exitTimer = setTimeout(() => {
      isOpen = false;
      dotNetRef.invokeMethodAsync(exitMethod);
    }, exitDelay);
  };

  // Hover on trigger or popup keeps the overlay open.
  triggerEl.addEventListener("pointerenter", scheduleOpen);
  triggerEl.addEventListener("pointerleave", scheduleClose);

  if (popupEl) {
    popupEl.addEventListener("pointerenter", () => {
      clearTimers();
    });
    popupEl.addEventListener("pointerleave", scheduleClose);
  }

  // Focus also opens/closes (keyboard accessibility).
  triggerEl.addEventListener("focusin", scheduleOpen);
  triggerEl.addEventListener("focusout", scheduleClose);

  return () => {
    clearTimers();
    triggerEl.removeEventListener("pointerenter", scheduleOpen);
    triggerEl.removeEventListener("pointerleave", scheduleClose);
    triggerEl.removeEventListener("focusin", scheduleOpen);
    triggerEl.removeEventListener("focusout", scheduleClose);
    if (popupEl) {
      popupEl.removeEventListener("pointerenter", () => {});
      popupEl.removeEventListener("pointerleave", scheduleClose);
    }
  };
}

// Escape key handler for overlays that need to intercept Escape.

export function onEscapeKey(element, dotNetRef, callbackMethod) {
  const handler = (e) => {
    if (e.key === "Escape") {
      e.preventDefault();
      e.stopPropagation();
      dotNetRef.invokeMethodAsync(callbackMethod);
    }
  };

  element.addEventListener("keydown", handler);
  return () => element.removeEventListener("keydown", handler);
}

// Dialog cancel event interception — intercepts native <dialog> cancel to run exit animation.

export function onDialogCancel(dialogEl, dotNetRef, callbackMethod) {
  const handler = (e) => {
    e.preventDefault();
    dotNetRef.invokeMethodAsync(callbackMethod);
  };

  dialogEl.addEventListener("cancel", handler);
  return () => dialogEl.removeEventListener("cancel", handler);
}

// Registration facade

const instances = new Map();

export function register(elementId, subsystems = {}) {
  const cleanups = [];
  const el = document.getElementById(elementId);
  if (!el) return null;

  if (subsystems.keyboardNav) {
    cleanups.push(initKeyboardNav(el, subsystems.keyboardNav));
  }

  if (subsystems.focusTrap) {
    cleanups.push(trapFocus(el));
  }

  if (subsystems.clickOutside && subsystems.clickOutside.dotNetRef) {
    const { dotNetRef, callbackMethod } = subsystems.clickOutside;
    cleanups.push(onClickOutside(el, dotNetRef, callbackMethod));
  }

  if (subsystems.positioning && subsystems.positioning.anchorId) {
    const anchor = document.getElementById(subsystems.positioning.anchorId);
    if (anchor) {
      const autoId = startAutoUpdate(anchor, el, subsystems.positioning);
      cleanups.push(() => stopAutoUpdate(autoId));
    }
  }

  if (subsystems.animation && subsystems.animation.animateEntry) {
    animateEntry(el);
  }

  const id = crypto.randomUUID();
  instances.set(id, () => cleanups.forEach((fn) => fn()));
  return id;
}

export function unregister(id) {
  const cleanup = instances.get(id);
  if (cleanup) {
    cleanup();
    instances.delete(id);
  }
}
