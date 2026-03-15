// Stepper and scrub area registrations are keyed by element ID so multiple
// NumberField instances on the same page don't interfere with each other.
// dispose(instanceKey) removes only the entries that belong to the given root.

import {
  registerDelegatedHandler,
  unregisterDelegatedHandler,
} from "/_content/BlazeUI.Bridge/blazeui.bridge.js";

const steppers = new Map();
const scrubAreas = new Map();

// Track which instance key a given element belongs to, so dispose() can
// find the right entries when cleaning up a specific root.
const stepperOwners = new Map();  // buttonId → instanceKey
const scrubOwners = new Map();    // scrubAreaId → instanceKey

export function initStepper(buttonId, ref, direction, instanceKey) {
  let intervalId = null;
  let timeoutId = null;

  const step = () => ref.invokeMethodAsync("OnStep", direction);

  const startRepeat = () => {
    step();
    // Initial delay before repeat starts, then fire at ~20fps.
    timeoutId = setTimeout(() => {
      intervalId = setInterval(step, 50);
    }, 300);
  };

  const stopRepeat = () => {
    if (timeoutId) { clearTimeout(timeoutId); timeoutId = null; }
    if (intervalId) { clearInterval(intervalId); intervalId = null; }
  };

  // Use delegated handlers for pointerdown so listeners survive Blazor's
  // SSR → interactive DOM replacement. pointerup is on document (ephemeral,
  // attached during press) and pointerout via delegation handles leave.
  registerDelegatedHandler("pointerdown", buttonId, () => {
    startRepeat();
    // Ephemeral pointerup on document — fires even if pointer leaves the button.
    const onUp = () => {
      stopRepeat();
      document.removeEventListener("pointerup", onUp);
    };
    document.addEventListener("pointerup", onUp);
  });

  // pointerout bubbles (unlike pointerleave). Filter child transitions by
  // checking relatedTarget is outside the button.
  registerDelegatedHandler("pointerout", buttonId, (e, el) => {
    if (!el.contains(e.relatedTarget)) {
      stopRepeat();
    }
  });

  steppers.set(buttonId, () => {
    stopRepeat();
    unregisterDelegatedHandler("pointerdown", buttonId);
    unregisterDelegatedHandler("pointerout", buttonId);
  });

  if (instanceKey) stepperOwners.set(buttonId, instanceKey);
}

/**
 * Initializes a scrub area that uses the Pointer Lock API for drag-to-adjust.
 * @param {string} scrubAreaId - ID of the scrub area element
 * @param {object} ref - DotNetObjectReference for OnStep/OnScrubStart/OnScrubEnd callbacks
 * @param {object} options - { direction, pixelSensitivity, teleportDistance, step, largeStep }
 */
export function initScrubArea(scrubAreaId, ref, options, instanceKey) {
  const { direction, pixelSensitivity, teleportDistance } = options;
  const isHorizontal = direction === "horizontal";
  let accumulated = 0;
  let isScrubbing = false;

  // WebKit doesn't support Pointer Lock well — fall back to raw movement tracking.
  const isWebKit = /^((?!chrome|android).)*safari/i.test(navigator.userAgent);

  // Look up the element by ID each time rather than caching a reference, so
  // pointer lock operations target the current DOM element after SSR handoff.
  const getEl = () => document.getElementById(scrubAreaId);

  const onPointerDown = (e) => {
    if (e.pointerType === "touch") return;
    if (e.button !== 0) return;

    accumulated = 0;
    isScrubbing = true;

    const el = getEl();
    if (!isWebKit && el) {
      el.requestPointerLock?.();
    }

    ref.invokeMethodAsync("OnScrubStart");

    document.addEventListener("pointermove", onPointerMove);
    document.addEventListener("pointerup", onPointerUp);
  };

  const onPointerMove = (e) => {
    if (!isScrubbing) return;

    const delta = isHorizontal ? e.movementX : -e.movementY;
    accumulated += delta;

    // Step the value when accumulated pixels exceed the sensitivity threshold.
    while (Math.abs(accumulated) >= pixelSensitivity) {
      const stepDir = accumulated > 0 ? 1 : -1;
      ref.invokeMethodAsync("OnStep", stepDir);
      accumulated -= stepDir * pixelSensitivity;
    }

    // Teleport cursor to opposite edge when it reaches viewport boundary.
    const el = getEl();
    if (teleportDistance && !isWebKit && el && document.pointerLockElement === el) {
      const cursorEl = document.querySelector(`[id$="scrub-cursor"]`);
      if (cursorEl) {
        const rect = cursorEl.getBoundingClientRect();
        const viewW = window.innerWidth;

        if (isHorizontal) {
          if (rect.left <= teleportDistance) {
            cursorEl.style.transform = `translate3d(${viewW - teleportDistance * 2}px, ${rect.top}px, 0)`;
          } else if (rect.left >= viewW - teleportDistance) {
            cursorEl.style.transform = `translate3d(${teleportDistance}px, ${rect.top}px, 0)`;
          }
        }
      }
    }
  };

  const onPointerUp = () => {
    if (!isScrubbing) return;
    isScrubbing = false;

    const el = getEl();
    if (!isWebKit && el && document.pointerLockElement === el) {
      document.exitPointerLock?.();
    }

    document.removeEventListener("pointermove", onPointerMove);
    document.removeEventListener("pointerup", onPointerUp);

    ref.invokeMethodAsync("OnScrubEnd");
  };

  // Use delegated handler so the listener survives SSR → interactive handoff.
  registerDelegatedHandler("pointerdown", scrubAreaId, onPointerDown);

  scrubAreas.set(scrubAreaId, () => {
    unregisterDelegatedHandler("pointerdown", scrubAreaId);
    document.removeEventListener("pointermove", onPointerMove);
    document.removeEventListener("pointerup", onPointerUp);
    const currentEl = document.getElementById(scrubAreaId);
    if (currentEl && document.pointerLockElement === currentEl) {
      document.exitPointerLock?.();
    }
  });

  if (instanceKey) scrubOwners.set(scrubAreaId, instanceKey);
}

/**
 * Cleans up all steppers and scrub areas registered under the given instance key.
 * Each NumberFieldRoot passes its own stable key so concurrent instances are isolated.
 * @param {string} instanceKey
 */
export function dispose(instanceKey) {
  // Clean up steppers that belong to this instance.
  for (const [buttonId, ownerKey] of stepperOwners.entries()) {
    if (ownerKey === instanceKey) {
      const cleanup = steppers.get(buttonId);
      if (cleanup) { cleanup(); steppers.delete(buttonId); }
      stepperOwners.delete(buttonId);
    }
  }

  // Clean up scrub areas that belong to this instance.
  for (const [scrubAreaId, ownerKey] of scrubOwners.entries()) {
    if (ownerKey === instanceKey) {
      const cleanup = scrubAreas.get(scrubAreaId);
      if (cleanup) { cleanup(); scrubAreas.delete(scrubAreaId); }
      scrubOwners.delete(scrubAreaId);
    }
  }

  // Fallback: if ownership wasn't tracked (e.g. single-instance usage), clear everything.
  if (stepperOwners.size === 0 && scrubOwners.size === 0) {
    for (const cleanup of steppers.values()) cleanup();
    steppers.clear();
    for (const cleanup of scrubAreas.values()) cleanup();
    scrubAreas.clear();
  }
}
