// Event Delegation Registry
//
// Provides a single document-level listener per event type that dispatches
// to registered handlers by element ID. This survives Blazor's SSR → interactive
// DOM replacement where element-scoped listeners would be lost.
//
// Usage guidance:
// - Registry (registerDelegatedHandler/unregisterDelegatedHandler): Use for
//   persistent listeners registered during init()/firstRender that must survive
//   the SSR → interactive DOM replacement. These are attached once and live for
//   the component's lifetime.
// - Raw addEventListener: Use for ephemeral listeners attached during user
//   interaction (e.g., pointermove during a drag, escape key during a dialog
//   show). These are created after the SSR handoff when the DOM is stable,
//   so they don't need delegation.

// Map<eventType, Map<elementId, handler>>
const registry = new Map();

// Map<eventType, listenerFn> — stored so we can call removeEventListener later
const listeners = new Map();

/**
 * Registers a delegated event handler for an element.
 *
 * The handler fires for ALL events where `element.contains(e.target)` is true.
 * This means any event originating from a descendant of the registered element
 * will trigger the handler. If the handler needs narrower scoping (e.g., only
 * direct children, or only elements matching a selector), it must check
 * `e.target` itself. Nested registered elements will both fire — there is no
 * stopPropagation between handlers in the registry.
 *
 * @param {string} eventType - DOM event type (e.g., "keydown", "click")
 * @param {string} elementId - ID of the element to scope events to
 * @param {(e: Event, element: Element) => void} handler - callback receiving the event and resolved element
 */
export function registerDelegatedHandler(eventType, elementId, handler) {
  if (!registry.has(eventType)) {
    registry.set(eventType, new Map());

    const listener = (e) => {
      const handlers = registry.get(eventType);
      if (!handlers) return;

      for (const [id, h] of handlers) {
        const el = document.getElementById(id);
        if (el && el.contains(e.target)) {
          h(e, el);
        }
      }
    };

    listeners.set(eventType, listener);
    document.addEventListener(eventType, listener);
  }

  registry.get(eventType).set(elementId, handler);
}

/**
 * Removes a previously registered delegated handler.
 *
 * @param {string} eventType - DOM event type
 * @param {string} elementId - ID of the element to unregister
 */
export function unregisterDelegatedHandler(eventType, elementId) {
  const handlers = registry.get(eventType);
  if (handlers) {
    handlers.delete(elementId);

    // Clean up the document-level listener when no handlers remain for this event type
    if (handlers.size === 0) {
      document.removeEventListener(eventType, listeners.get(eventType));
      listeners.delete(eventType);
      registry.delete(eventType);
    }
  }
}
