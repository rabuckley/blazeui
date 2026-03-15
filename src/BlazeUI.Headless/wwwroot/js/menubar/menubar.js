import {
  registerDelegatedHandler,
  unregisterDelegatedHandler,
} from "/_content/BlazeUI.Bridge/blazeui.bridge.js";

const instances = new Map();

export function init(menubarId, ref) {
  const existing = instances.get(menubarId);
  if (existing) {
    unregisterDelegatedHandler("keydown", menubarId);
  }

  instances.set(menubarId, { dotNetRef: ref });

  registerDelegatedHandler("keydown", menubarId, (e, menubar) => {
    const triggers = [...menubar.querySelectorAll('[role="menuitem"]')];
    if (triggers.length === 0) return;

    const currentIndex = triggers.indexOf(document.activeElement);

    switch (e.key) {
      case "ArrowRight": {
        e.preventDefault();
        if (currentIndex !== -1) {
          const hasOpenMenu = menubar.querySelector('[aria-expanded="true"]');
          if (hasOpenMenu) {
            ref.invokeMethodAsync("OnNavigateMenu", 1);
          } else {
            const next = currentIndex < triggers.length - 1 ? currentIndex + 1 : 0;
            triggers[next].focus();
          }
        }
        break;
      }
      case "ArrowLeft": {
        e.preventDefault();
        if (currentIndex !== -1) {
          const hasOpenMenu = menubar.querySelector('[aria-expanded="true"]');
          if (hasOpenMenu) {
            ref.invokeMethodAsync("OnNavigateMenu", -1);
          } else {
            const prev = currentIndex > 0 ? currentIndex - 1 : triggers.length - 1;
            triggers[prev].focus();
          }
        }
        break;
      }
      case "ArrowDown": {
        e.preventDefault();
        if (document.activeElement && triggers.includes(document.activeElement)) {
          document.activeElement.click();
        }
        break;
      }
      case "Escape": {
        e.preventDefault();
        ref.invokeMethodAsync("OnEscapeKey");
        break;
      }
      case "Home": {
        e.preventDefault();
        triggers[0].focus();
        break;
      }
      case "End": {
        e.preventDefault();
        triggers[triggers.length - 1].focus();
        break;
      }
    }
  });
}

export function focusTrigger(triggerId) {
  const trigger = document.getElementById(triggerId);
  if (trigger) trigger.focus();
}

export function dispose(menubarId) {
  const inst = instances.get(menubarId);
  if (!inst) return;
  unregisterDelegatedHandler("keydown", menubarId);
  instances.delete(menubarId);
}
