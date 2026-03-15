// JS module for InputOTP headless component.
// Manages the hidden native input's events and selection tracking,
// reporting state changes back to the Blazor component via interop.

const instances = new Map();

export function init(inputId, dotNetRef) {
  // Clean up any previous instance for this input.
  dispose(inputId);

  const handlers = [];

  function on(target, event, handler, options) {
    target.addEventListener(event, handler, options);
    handlers.push(() => target.removeEventListener(event, handler, options));
  }

  // Re-query the input on each event to survive SSR → interactive DOM replacement.
  function getInput() {
    return document.getElementById(inputId);
  }

  // Report the current selection range to Blazor.
  function reportSelection() {
    const el = getInput();
    if (!el) return;
    const start = el.selectionStart ?? 0;
    const end = el.selectionEnd ?? 0;
    dotNetRef.invokeMethodAsync("OnSelectionChanged", start, end);
  }

  // Value changes (keyboard input, paste, autofill).
  on(document, "input", (e) => {
    const el = getInput();
    if (!el || e.target !== el) return;
    dotNetRef.invokeMethodAsync("OnInputValueChanged", el.value);
    // Selection may have moved after input — report after a tick.
    requestAnimationFrame(reportSelection);
  });

  // Selection tracking — the document-level selectionchange event is the
  // most reliable way to track caret movement across all browsers.
  on(document, "selectionchange", () => {
    const el = getInput();
    if (!el || document.activeElement !== el) return;
    reportSelection();
  });

  // Focus / blur.
  on(document, "focusin", (e) => {
    const el = getInput();
    if (!el || e.target !== el) return;
    dotNetRef.invokeMethodAsync("OnFocusChanged", true);
    requestAnimationFrame(reportSelection);
  });

  on(document, "focusout", (e) => {
    const el = getInput();
    if (!el || e.target !== el) return;
    dotNetRef.invokeMethodAsync("OnFocusChanged", false);
  });

  // Track container height and set --root-height on both the container and
  // the input so font-size: var(--root-height) resolves correctly. The
  // reference input-otp library does the same via ResizeObserver.
  const inputEl = getInput();
  if (inputEl) {
    const container = inputEl.closest("[data-input-otp-container]") || inputEl.parentElement?.parentElement;
    if (container) {
      const updateHeight = () => {
        const h = `${container.getBoundingClientRect().height}px`;
        container.style.setProperty("--root-height", h);
        // Set font-size directly — avoids CSS variable resolution timing issues.
        inputEl.style.fontSize = h;
      };
      const ro = new ResizeObserver(updateHeight);
      ro.observe(container);
      // Initial measurement in case ResizeObserver hasn't fired yet.
      updateHeight();
      handlers.push(() => ro.disconnect());
    }
  }

  instances.set(inputId, () => {
    for (const off of handlers) off();
    handlers.length = 0;
  });
}

export function focusInput(inputId) {
  const el = document.getElementById(inputId);
  if (el) {
    el.focus();
    // Place cursor at end of current value.
    const len = el.value.length;
    el.setSelectionRange(len, len);
  }
}

export function dispose(inputId) {
  const cleanup = instances.get(inputId);
  if (!cleanup) return;
  cleanup();
  instances.delete(inputId);
}
