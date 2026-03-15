// Sidebar JS module — handles mobile viewport detection.
// Per-instance state so multiple SidebarProviders on the same page
// don't clobber each other.

const MOBILE_BREAKPOINT = 768;

const instances = new Map();

export function initMobile(instanceKey, ref) {
  const mql = window.matchMedia(`(max-width: ${MOBILE_BREAKPOINT - 1}px)`);
  const onChangeHandler = () =>
    ref.invokeMethodAsync(
      "OnMobileChanged",
      window.innerWidth < MOBILE_BREAKPOINT
    );
  mql.addEventListener("change", onChangeHandler);
  instances.set(instanceKey, { mql, onChangeHandler, dotNetRef: ref });
  return window.innerWidth < MOBILE_BREAKPOINT;
}

export function dispose(instanceKey) {
  const inst = instances.get(instanceKey);
  if (!inst) return;
  inst.mql.removeEventListener("change", inst.onChangeHandler);
  instances.delete(instanceKey);
}
