import {
  onClickOutside,
  onEscapeKey,
  position,
  startAutoUpdate,
  stopAutoUpdate,
  animateEntry,
  showPopover,
  hidePopover,
  restoreFocus,
  showPortalEntry,
  hidePortalEntry,
} from "/_content/BlazeUI.Headless/blazeui.core.js";

// Per-instance state keyed by popupId. Supports multiple comboboxes on the same
// page without module-level variable collisions.
const instances = new Map();

// Combobox-specific keyboard navigation. Unlike menu keyboard nav, the combobox
// keeps focus on the input and tracks the highlighted item via data attributes.
// Arrow keys cycle through visible options; Enter selects the highlighted one.
//
// When `inlineComplete` is true (Autocomplete Both/Inline modes), highlighting
// an item temporarily sets the input value to the item's display text and selects
// the completion portion so the user can keep typing to narrow further or press
// Enter to accept.
function initComboboxKeyboard(inputEl, popupEl, ref, inlineComplete) {
  let highlightedIndex = -1;

  // The text the user actually typed, tracked separately from the input value
  // which may temporarily show inline-completed text.
  let typedText = inputEl.value;

  const getVisibleItems = () =>
    Array.from(
      popupEl.querySelectorAll(
        '[role="option"]:not([aria-disabled="true"]):not([style*="display:none"]):not([style*="display: none"])'
      )
    );

  const applyInlineCompletion = (item) => {
    if (!inlineComplete || !item) return;
    const label = item.dataset.label || item.textContent.trim();
    // Only complete inline when the item starts with what the user typed
    // (case-insensitive). Otherwise the completion text wouldn't make sense.
    if (label.toLowerCase().startsWith(typedText.toLowerCase())) {
      // Preserve the user's casing for the typed portion, append the rest.
      inputEl.value = typedText + label.substring(typedText.length);
      inputEl.setSelectionRange(typedText.length, inputEl.value.length);
    } else {
      inputEl.value = label;
      inputEl.setSelectionRange(0, label.length);
    }
  };

  const clearInlineCompletion = () => {
    if (!inlineComplete) return;
    inputEl.value = typedText;
  };

  const setHighlight = (items, index) => {
    // Clear previous highlight.
    items.forEach((item) => item.removeAttribute("data-highlighted"));
    highlightedIndex = index;

    if (index >= 0 && index < items.length) {
      items[index].setAttribute("data-highlighted", "");
      items[index].scrollIntoView({ block: "nearest" });
      applyInlineCompletion(items[index]);
      ref.invokeMethodAsync("OnHighlightChange", items[index].id);
    } else {
      clearInlineCompletion();
      ref.invokeMethodAsync("OnHighlightChange", null);
    }
  };

  // Track user typing to keep typedText current. The `input` event fires on
  // user edits (typing, pasting, cutting) but not on programmatic value changes.
  const inputHandler = () => {
    typedText = inputEl.value;
    // Reset highlight when the user types — stale highlights don't match the new filter.
    highlightedIndex = -1;
  };
  inputEl.addEventListener("input", inputHandler);

  const handler = (e) => {
    const items = getVisibleItems();
    if (items.length === 0) return;

    switch (e.key) {
      case "ArrowDown": {
        e.preventDefault();
        const next =
          highlightedIndex < items.length - 1 ? highlightedIndex + 1 : 0;
        setHighlight(items, next);
        break;
      }
      case "ArrowUp": {
        e.preventDefault();
        const prev =
          highlightedIndex > 0 ? highlightedIndex - 1 : items.length - 1;
        setHighlight(items, prev);
        break;
      }
      case "Home": {
        e.preventDefault();
        setHighlight(items, 0);
        break;
      }
      case "End": {
        e.preventDefault();
        setHighlight(items, items.length - 1);
        break;
      }
      case "Enter": {
        if (highlightedIndex >= 0 && highlightedIndex < items.length) {
          e.preventDefault();
          // For inline completion, commit the completed text as the typed text
          // before clicking so that SelectItem sees the final value.
          if (inlineComplete) {
            typedText = inputEl.value;
          }
          items[highlightedIndex].click();
        }
        break;
      }
    }
  };

  // Listen on the input — focus stays there during combobox interaction.
  inputEl.addEventListener("keydown", handler);
  return () => {
    inputEl.removeEventListener("keydown", handler);
    inputEl.removeEventListener("input", inputHandler);
    // Clear any lingering highlight.
    const items = getVisibleItems();
    items.forEach((item) => item.removeAttribute("data-highlighted"));
  };
}

export async function show(inputId, popupId, positionerId, options, ref) {
  let state = instances.get(popupId);
  if (!state) {
    state = {
      clickOutsideCleanup: null,
      escapeCleanup: null,
      keyboardNavCleanup: null,
      autoUpdateId: null,
      styleObserver: null,
      dotNetRef: null,
      hideTimer: null,
    };
    instances.set(popupId, state);
  }
  state.dotNetRef = ref;

  const input = document.getElementById(inputId);
  const popup = document.getElementById(popupId);
  if (!input || !popup) return;

  // Cancel any pending hide timer from a previous close — a quick reopen
  // would otherwise be undone by the delayed hidePopover() call.
  if (state.hideTimer) {
    clearTimeout(state.hideTimer);
    state.hideTimer = null;
  }

  // Make the popup visible. The popup is already portaled to the body via
  // BlazeUI's PortalHost, so no Popover API is needed for layering — the
  // z-50 class on the Positioner handles stacking. Popover API's UA styles
  // (`inset: 0`) override inline left/top positioning from floating-ui.

  // Position BEFORE setting data-open so the CSS animate-in animation
  // starts from the correct anchor-relative coordinates, not (0, 0).
  const positionOptions = {
    placement: options.placement,
    offset: options.offset,
  };
  await position(input, popup, positionOptions);

  // The core position() function sets --transform-origin on the popup, but the
  // reference's combobox popup uses the CSS default (50% 50% center) because the
  // variable isn't defined in its cascade. Remove it after each auto-update cycle.
  const clearOrigin = () => popup.style.removeProperty("--transform-origin");
  clearOrigin();

  if (state.styleObserver) state.styleObserver.disconnect();
  state.styleObserver = new MutationObserver(() => clearOrigin());
  state.styleObserver.observe(popup, { attributes: true, attributeFilter: ["style"] });

  // Now start auto-update for subsequent scroll/resize repositioning.
  if (state.autoUpdateId) stopAutoUpdate(state.autoUpdateId);
  state.autoUpdateId = startAutoUpdate(input, popup, positionOptions);

  // Set data-open AFTER positioning — this triggers the CSS animate-in
  // animation from the correct position. data-open is JS-owned; the C#
  // BuildRenderTree intentionally omits it so Blazor doesn't trigger the
  // animation before positioning completes.
  popup.removeAttribute("data-closed");
  showPortalEntry(popup);
  popup.setAttribute("data-open", "");
  animateEntry(popup);

  // Combobox keyboard nav — focus stays on input, highlights via data attrs.
  if (state.keyboardNavCleanup) state.keyboardNavCleanup();
  state.keyboardNavCleanup = initComboboxKeyboard(
    input, popup, ref, !!options.inlineComplete
  );

  // Click outside to close — exclude the input's InputGroup parent so that
  // clicks on the trigger button don't race with the Blazor onclick handler.
  if (state.clickOutsideCleanup) state.clickOutsideCleanup();
  const anchor = input.closest("[data-slot=input-group]") || input;
  setTimeout(() => {
    const handler = (e) => {
      if (!popup.contains(e.target) && !anchor.contains(e.target)) {
        ref.invokeMethodAsync("OnClickOutside");
      }
    };
    document.addEventListener("pointerdown", handler, true);
    state.clickOutsideCleanup = () =>
      document.removeEventListener("pointerdown", handler, true);
  }, 0);

  // Escape at document level for robustness.
  if (state.escapeCleanup) state.escapeCleanup();
  state.escapeCleanup = onEscapeKey(document, ref, "OnEscapeKey");
}

export function hide(popupId, positionerId) {
  const state = instances.get(popupId);
  const popup = document.getElementById(popupId);
  cleanupListeners(popupId);

  if (!popup) {
    state?.dotNetRef?.invokeMethodAsync("OnExitAnimationComplete");
    return;
  }

  // Set data-closed / remove data-open directly on DOM before Blazor
  // re-renders, so the subsequent BuildRenderTree is a no-op.
  popup.removeAttribute("data-open");
  popup.setAttribute("data-closed", "");

  // The popup uses CSS animations (animate-out) not transitions, so
  // listen for animationend to hide the popover immediately when the
  // exit animation finishes. The 350ms timeout is a safety fallback
  // in case no animation is defined.
  let hidden = false;
  const hideOnce = () => {
    if (hidden) return;
    hidden = true;
    if (state) state.hideTimer = null;
    // Immediately hide the element to prevent a single-frame flash at full
    // opacity between the CSS exit animation completing and Blazor unmounting
    // the element (the Blazor roundtrip takes ~8ms on localhost).
    popup.style.display = "none";
    hidePortalEntry(popup);
    state?.dotNetRef?.invokeMethodAsync("OnExitAnimationComplete");
  };

  popup.addEventListener("animationend", hideOnce, { once: true });

  if (state) {
    state.hideTimer = setTimeout(() => {
      popup.removeEventListener("animationend", hideOnce);
      hideOnce();
    }, 350);
  } else {
    setTimeout(() => {
      popup.removeEventListener("animationend", hideOnce);
      hideOnce();
    }, 350);
  }
}

function cleanupListeners(popupId) {
  const state = instances.get(popupId);
  if (!state) return;

  if (state.clickOutsideCleanup) {
    state.clickOutsideCleanup();
    state.clickOutsideCleanup = null;
  }
  if (state.escapeCleanup) {
    state.escapeCleanup();
    state.escapeCleanup = null;
  }
  if (state.keyboardNavCleanup) {
    state.keyboardNavCleanup();
    state.keyboardNavCleanup = null;
  }
  if (state.styleObserver) {
    state.styleObserver.disconnect();
    state.styleObserver = null;
  }
  if (state.autoUpdateId) {
    stopAutoUpdate(state.autoUpdateId);
    state.autoUpdateId = null;
  }
}

/// Sets the text selection range on the input element. Used by AutocompleteRoot
/// after Blazor re-renders to restore the inline completion selection that
/// Blazor's value attribute update may have cleared.
export function setInputSelection(inputId, start, end) {
  const input = document.getElementById(inputId);
  if (input) input.setSelectionRange(start, end);
}

export function dispose(popupId) {
  const state = instances.get(popupId);
  if (!state) return;

  cleanupListeners(popupId);
  if (state.hideTimer) {
    clearTimeout(state.hideTimer);
    state.hideTimer = null;
  }
  instances.delete(popupId);
}
