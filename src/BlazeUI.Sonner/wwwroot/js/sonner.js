// BlazeUI.Sonner JS module — minimal interop for height measurement,
// swipe gestures, and document visibility change detection.

const SWIPE_THRESHOLD = 45;

// -- Toaster-level init/dispose --

const visibilityListeners = new Map();

/**
 * Register a visibility change listener for the Toaster component.
 * @param {string} sectionId - The Toaster's element ID.
 * @param {object} dotNetRef - DotNetObjectReference for callbacks.
 */
export function init(sectionId, dotNetRef) {
    const handler = () => {
        dotNetRef.invokeMethodAsync('OnVisibilityChange', document.hidden);
    };
    document.addEventListener('visibilitychange', handler);
    visibilityListeners.set(sectionId, handler);
}

/**
 * Remove the visibility change listener.
 * @param {string} sectionId
 */
export function dispose(sectionId) {
    const handler = visibilityListeners.get(sectionId);
    if (handler) {
        document.removeEventListener('visibilitychange', handler);
        visibilityListeners.delete(sectionId);
    }

    // Clean up only swipe handlers belonging to this Toaster.
    for (const [id, state] of swipeStates) {
        if (state.sectionId === sectionId) {
            cleanupSwipe(id);
        }
    }
}

// -- Height measurement --

/**
 * Measures the rendered height of a toast element using requestAnimationFrame
 * to ensure layout is complete.
 * @param {string} toastId - The toast element's ID.
 * @returns {Promise<number>} The measured offsetHeight.
 */
export function measureHeight(toastId) {
    return new Promise((resolve) => {
        requestAnimationFrame(() => {
            const el = document.getElementById(toastId);
            if (!el) { resolve(0); return; }

            // The CSS rule `height: var(--front-toast-height)` may have
            // collapsed non-front toasts. Temporarily override to measure
            // the natural height, matching Sonner's React useLayoutEffect.
            const original = el.style.height;
            el.style.height = 'auto';
            // Use offsetHeight, not getBoundingClientRect().height, because
            // getBoundingClientRect includes CSS transforms (scale) which
            // would return a smaller value for non-front scaled-down toasts.
            const height = el.offsetHeight;
            el.style.height = original;
            resolve(height);
        });
    });
}

// -- Swipe gesture handling --
// Pointer move events are handled entirely in JS to avoid excessive
// SignalR round-trips on Blazor Server. Only the final dismiss decision
// crosses to C# via the dotNetRef callback.

const swipeStates = new Map();

/**
 * Initialize swipe gesture tracking on a toast element.
 * @param {string} toastId - The toast element's ID.
 * @param {object} dotNetRef - DotNetObjectReference for OnSwipeDismiss/OnSwipeStateChange.
 * @param {string} sectionId - The owning Toaster's section ID, used for scoped cleanup.
 */
export function initSwipe(toastId, dotNetRef, sectionId) {
    const el = document.getElementById(toastId);
    if (!el) return;

    const state = {
        dotNetRef,
        el,
        sectionId,
        pointerStart: null,
        dragStartTime: null,
        swipeDirection: null, // 'x' or 'y'
        onPointerDown: null,
        onPointerMove: null,
        onPointerUp: null,
        onPointerCancel: null,
    };

    state.onPointerDown = (e) => {
        if (e.button === 2) return; // Ignore right-click
        if (el.dataset.dismissible === 'false') return;
        if (el.dataset.type === 'loading') return;
        if (e.target.tagName === 'BUTTON') return;

        state.pointerStart = { x: e.clientX, y: e.clientY };
        state.dragStartTime = Date.now();
        state.swipeDirection = null;
        el.setPointerCapture(e.pointerId);

        dotNetRef.invokeMethodAsync('OnSwipeStateChange', true).catch(() => {});
    };

    state.onPointerMove = (e) => {
        if (!state.pointerStart) return;

        const xDelta = e.clientX - state.pointerStart.x;
        const yDelta = e.clientY - state.pointerStart.y;

        // Determine direction once
        if (!state.swipeDirection && (Math.abs(xDelta) > 1 || Math.abs(yDelta) > 1)) {
            state.swipeDirection = Math.abs(xDelta) > Math.abs(yDelta) ? 'x' : 'y';
        }

        const getDampening = (delta) => 1 / (1.5 + Math.abs(delta) / 20);

        // Determine which directions are allowed based on position data attributes
        const yPosition = el.dataset.yPosition;
        const xPosition = el.dataset.xPosition;
        const allowedDirs = [];
        if (yPosition) allowedDirs.push(yPosition);
        if (xPosition && xPosition !== 'center') allowedDirs.push(xPosition);

        let swipeX = 0;
        let swipeY = 0;

        if (state.swipeDirection === 'y') {
            if (allowedDirs.includes('top') || allowedDirs.includes('bottom')) {
                if ((allowedDirs.includes('top') && yDelta < 0) || (allowedDirs.includes('bottom') && yDelta > 0)) {
                    swipeY = yDelta;
                } else {
                    const dampened = yDelta * getDampening(yDelta);
                    swipeY = Math.abs(dampened) < Math.abs(yDelta) ? dampened : yDelta;
                }
            }
        } else if (state.swipeDirection === 'x') {
            if (allowedDirs.includes('left') || allowedDirs.includes('right')) {
                if ((allowedDirs.includes('left') && xDelta < 0) || (allowedDirs.includes('right') && xDelta > 0)) {
                    swipeX = xDelta;
                } else {
                    const dampened = xDelta * getDampening(xDelta);
                    swipeX = Math.abs(dampened) < Math.abs(xDelta) ? dampened : xDelta;
                }
            }
        }

        el.style.setProperty('--swipe-amount-x', `${swipeX}px`);
        el.style.setProperty('--swipe-amount-y', `${swipeY}px`);
    };

    state.onPointerUp = () => {
        if (!state.pointerStart) return;

        const swipeAmountX = parseFloat(el.style.getPropertyValue('--swipe-amount-x')) || 0;
        const swipeAmountY = parseFloat(el.style.getPropertyValue('--swipe-amount-y')) || 0;
        const timeTaken = Date.now() - state.dragStartTime;
        const swipeAmount = state.swipeDirection === 'x' ? swipeAmountX : swipeAmountY;
        const velocity = Math.abs(swipeAmount) / timeTaken;

        if (Math.abs(swipeAmount) >= SWIPE_THRESHOLD || (Math.abs(swipeAmount) > 5 && velocity > 0.11)) {
            // Determine dismiss direction
            let direction;
            if (state.swipeDirection === 'x') {
                direction = swipeAmountX > 0 ? 'right' : 'left';
            } else {
                direction = swipeAmountY > 0 ? 'down' : 'up';
            }

            dotNetRef.invokeMethodAsync('OnSwipeDismiss', direction).catch(() => {});
        } else {
            // Reset swipe
            el.style.setProperty('--swipe-amount-x', '0px');
            el.style.setProperty('--swipe-amount-y', '0px');
        }

        state.pointerStart = null;
        state.swipeDirection = null;
        dotNetRef.invokeMethodAsync('OnSwipeStateChange', false).catch(() => {});
    };

    state.onPointerCancel = () => {
        state.pointerStart = null;
        state.swipeDirection = null;
        el.style.setProperty('--swipe-amount-x', '0px');
        el.style.setProperty('--swipe-amount-y', '0px');
        dotNetRef.invokeMethodAsync('OnSwipeStateChange', false).catch(() => {});
    };

    el.addEventListener('pointerdown', state.onPointerDown);
    el.addEventListener('pointermove', state.onPointerMove);
    el.addEventListener('pointerup', state.onPointerUp);
    el.addEventListener('pointercancel', state.onPointerCancel);

    swipeStates.set(toastId, state);
}

/**
 * Clean up swipe handlers for a toast element.
 * @param {string} toastId
 */
export function disposeSwipe(toastId) {
    cleanupSwipe(toastId);
}

function cleanupSwipe(toastId) {
    const state = swipeStates.get(toastId);
    if (!state) return;

    state.el.removeEventListener('pointerdown', state.onPointerDown);
    state.el.removeEventListener('pointermove', state.onPointerMove);
    state.el.removeEventListener('pointerup', state.onPointerUp);
    state.el.removeEventListener('pointercancel', state.onPointerCancel);
    swipeStates.delete(toastId);
}
