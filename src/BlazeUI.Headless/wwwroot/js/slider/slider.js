import { registerDelegatedHandler, unregisterDelegatedHandler } from "/_content/BlazeUI.Bridge/blazeui.bridge.js";

// Each slider instance is stored by controlId so multiple sliders on one page
// don't clobber each other's state.
const instances = new Map();

// Track which instance is currently being dragged so document-level
// pointermove/pointerup can route to the right slider.
let activeInstance = null;
let activeDragThumbIndex = -1;

export function init(controlId, thumbInputIds, opts, dotNetRef) {
  const controlEl = document.getElementById(controlId);
  if (!controlEl) return;

  const instance = {
    controlId,
    controlEl,
    thumbInputIds,
    options: opts,
    ref: dotNetRef,
  };

  // Pointer-down on the control starts a drag interaction. We capture it at the
  // document level via the delegation registry so the listener survives the SSR →
  // interactive DOM replacement.
  const onPointerDown = (e, controlElement) => {
    const opts = instance.options;
    if (opts.disabled) return;
    if (e.button !== 0) return;
    if (e.defaultPrevented) return;

    activeDragThumbIndex = resolveClosestThumbIndex(instance, e);
    activeInstance = instance;

    instance.ref.invokeMethodAsync("OnDragStart");
    updateValueFromPointer(instance, e, activeDragThumbIndex);

    // Ephemeral pointermove/pointerup listeners are attached here (after user
    // interaction) so they don't need event delegation — the DOM is stable at
    // this point and won't be replaced by the SSR handoff.
    document.addEventListener("pointermove", onPointerMove, { passive: true });
    document.addEventListener("pointerup", onPointerUp, { once: true });

    e.preventDefault();
  };

  const onPointerMove = (e) => {
    if (activeInstance !== instance) return;
    if (e.buttons === 0) {
      // Pointer was released without a pointerup (e.g. moved outside window).
      finishDrag(instance, e);
      return;
    }
    updateValueFromPointer(instance, e, activeDragThumbIndex);
  };

  const onPointerUp = (e) => {
    if (activeInstance !== instance) return;
    finishDrag(instance, e);
  };

  // Keyboard navigation on the thumb input elements. Registered via delegation
  // so it survives SSR → interactive replacement.
  const onKeyDown = (e, controlElement) => {
    // Route only to inputs that belong to this control.
    const input = e.target;
    if (!instance.thumbInputIds.includes(input.id)) return;

    const thumbIndex = instance.thumbInputIds.indexOf(input.id);
    if (thumbIndex < 0) return;

    const { step, min, max, largeStep } = instance.options;
    const current = parseFloat(input.getAttribute("aria-valuenow")) || min;
    let newValue = null;

    switch (e.key) {
      case "ArrowRight":
      case "ArrowUp":
        newValue = current + (e.shiftKey ? largeStep : step);
        break;
      case "ArrowLeft":
      case "ArrowDown":
        newValue = current - (e.shiftKey ? largeStep : step);
        break;
      case "PageUp":
        newValue = current + largeStep;
        break;
      case "PageDown":
        newValue = current - largeStep;
        break;
      case "Home":
        newValue = min;
        break;
      case "End":
        newValue = max;
        break;
    }

    if (newValue !== null) {
      e.preventDefault();
      newValue = snapToStep(newValue, step, min, max);
      instance.ref.invokeMethodAsync("OnValueChange", newValue, thumbIndex);
    }
  };

  // Register persistent listeners via the delegation registry so they survive
  // the SSR → interactive DOM replacement.
  registerDelegatedHandler("pointerdown", controlId, onPointerDown);
  registerDelegatedHandler("keydown", controlId, onKeyDown);

  instance.onPointerDown = onPointerDown;
  instance.onPointerMove = onPointerMove;
  instance.onPointerUp = onPointerUp;
  instance.onKeyDown = onKeyDown;

  instances.set(controlId, instance);

  // Measure thumb sizes after the browser has laid out the elements and set
  // --slider-thumb-size so the CSS edge-compensation formula works correctly.
  requestAnimationFrame(() => measureThumbs(instance));
}

function finishDrag(instance, e) {
  activeInstance = null;
  activeDragThumbIndex = -1;
  instance.ref.invokeMethodAsync("OnDragEnd");
  document.removeEventListener("pointermove", instance.onPointerMove);
  document.removeEventListener("pointerup", instance.onPointerUp);
}

// Measure each thumb element and report the size back to C# so SliderThumb
// can include --slider-thumb-size in its rendered inline style (JS-set inline
// properties are wiped by Blazor's render diff). Also caches the size on the
// instance for pointer-to-value calculations.
function measureThumbs(instance) {
  const isHorizontal = instance.options.orientation !== "vertical";
  let maxSize = 0;

  for (const inputId of instance.thumbInputIds) {
    const input = document.getElementById(inputId);
    if (!input) continue;
    const thumb = input.parentElement;
    if (!thumb) continue;
    const rect = thumb.getBoundingClientRect();
    const size = isHorizontal ? rect.width : rect.height;
    if (size > maxSize) maxSize = size;
  }

  instance.thumbSize = maxSize;

  // Report to C# so the CSS variable is included in the rendered style and
  // survives Blazor re-renders.
  if (maxSize > 0) {
    instance.ref.invokeMethodAsync("OnThumbSizeMeasured", maxSize);
  }
}

function resolveClosestThumbIndex(instance, e) {
  // If the user pressed directly on a thumb input's container, use that index.
  const controlEl = document.getElementById(instance.controlId);
  if (!controlEl) return 0;

  for (let i = 0; i < instance.thumbInputIds.length; i++) {
    const input = document.getElementById(instance.thumbInputIds[i]);
    if (input && input.closest("[data-index]")?.contains(e.target)) {
      return i;
    }
  }

  // Otherwise find the thumb closest to the pointer position.
  const rect = controlEl.getBoundingClientRect();
  const isHorizontal = instance.options.orientation !== "vertical";
  const pointerPos = isHorizontal ? e.clientX - rect.left : rect.bottom - e.clientY;
  const controlSize = isHorizontal ? rect.width : rect.height;
  const thumbSize = instance.thumbSize || 0;
  const effectiveStart = thumbSize / 2;
  const effectiveSize = controlSize - thumbSize;

  let closestIndex = 0;
  let minDist = Infinity;

  for (let i = 0; i < instance.thumbInputIds.length; i++) {
    const val = parseFloat(
      document.getElementById(instance.thumbInputIds[i])?.getAttribute("aria-valuenow") ??
      String(instance.options.min)
    );
    const { min, max } = instance.options;
    // Map value to pixel position within the effective (inset) range.
    const thumbPos = effectiveStart + (val - min) / Math.max(max - min, 1) * effectiveSize;
    const dist = Math.abs(pointerPos - thumbPos);
    if (dist < minDist) {
      minDist = dist;
      closestIndex = i;
    }
  }

  return closestIndex;
}

function updateValueFromPointer(instance, e, thumbIndex) {
  const controlEl = document.getElementById(instance.controlId);
  if (!controlEl) return;

  const rect = controlEl.getBoundingClientRect();
  const isHorizontal = instance.options.orientation !== "vertical";
  const controlSize = isHorizontal ? rect.width : rect.height;
  if (controlSize === 0) return;

  // Account for thumb size: the CSS positions the thumb center between
  // thumbSize/2 and (controlSize - thumbSize/2), so map the pointer
  // within that effective range.
  const thumbSize = instance.thumbSize || 0;
  const effectiveStart = thumbSize / 2;
  const effectiveSize = controlSize - thumbSize;

  const rawOffset = isHorizontal
    ? e.clientX - rect.left
    : rect.bottom - e.clientY;
  const pixelOffset = rawOffset - effectiveStart;

  const percent = effectiveSize > 0
    ? Math.max(0, Math.min(1, pixelOffset / effectiveSize))
    : 0;
  const { min, max, step } = instance.options;
  const raw = min + percent * (max - min);
  const snapped = snapToStep(raw, step, min, max);

  instance.ref.invokeMethodAsync("OnValueChange", snapped, thumbIndex >= 0 ? thumbIndex : 0);
}

function snapToStep(value, step, min, max) {
  const snapped = Math.round((value - min) / step) * step + min;
  return Math.max(min, Math.min(max, snapped));
}

export function dispose(controlId) {
  const instance = instances.get(controlId);
  if (!instance) return;

  unregisterDelegatedHandler("pointerdown", controlId);
  unregisterDelegatedHandler("keydown", controlId);

  document.removeEventListener("pointermove", instance.onPointerMove);
  document.removeEventListener("pointerup", instance.onPointerUp);

  if (activeInstance === instance) {
    activeInstance = null;
    activeDragThumbIndex = -1;
  }

  instances.delete(controlId);
}
