import { animateEntry, animateExit } from "/_content/BlazeUI.Headless/blazeui.core.js";

const instances = new Map();

export function open(panelId, initialRender) {
  const panel = document.getElementById(panelId);
  if (!panel) return;

  let inst = instances.get(panelId);
  if (!inst) { inst = {}; instances.set(panelId, inst); }

  // Remove hidden to allow measurement.
  panel.removeAttribute("hidden");

  if (initialRender) {
    // DefaultOpen panels start open without animation, matching Base UI.
    // Set height to 'auto' so CSS animations that reference it (e.g.
    // collapsible-down from tw-animate-css) can't interpolate 0→auto
    // and cancel any CSS animation Blazor's data-[open] may have triggered.
    panel.style.setProperty("--collapsible-panel-height", "auto");
    // Override the CSS-rule animation with an inline 'none' to prevent
    // the slide-down animation from playing on page load.
    panel.style.animation = "none";
  } else {
    // Clear any initial-render animation suppression so CSS animations
    // work for subsequent open/close cycles.
    panel.style.animation = "";

    // Clip overflow during the expand animation so content doesn't
    // bleed out while height interpolates. Clear after completion,
    // matching Base UI's CollapsiblePanel behavior.
    panel.style.overflow = "hidden";

    // Measure and expose height as a CSS custom property so consumers
    // can animate `height` or `max-height` via CSS transitions.
    const height = panel.scrollHeight;
    panel.style.setProperty("--collapsible-panel-height", `${height}px`);
    animateEntry(panel);

    const anims = panel.getAnimations();
    if (anims.length > 0) {
      Promise.all(anims.map(a => a.finished)).then(() => {
        panel.style.overflow = "";
      }).catch(() => {});
    } else {
      panel.style.overflow = "";
    }
  }

  // Observe for content changes so the custom property stays accurate.
  // For initialRender, skip the first callback (ResizeObserver fires
  // immediately on observe) to preserve the 'auto' value we just set.
  if (inst.resizeObserver) inst.resizeObserver.disconnect();
  let skipFirst = initialRender;
  inst.resizeObserver = new ResizeObserver(() => {
    if (skipFirst) { skipFirst = false; return; }
    const h = panel.scrollHeight;
    panel.style.setProperty("--collapsible-panel-height", `${h}px`);
  });
  inst.resizeObserver.observe(panel);
}

export function close(panelId, dotNetRef) {
  const panel = document.getElementById(panelId);
  const inst = instances.get(panelId);
  if (inst?.resizeObserver) {
    inst.resizeObserver.disconnect();
    inst.resizeObserver = null;
  }

  if (!panel) {
    dotNetRef?.invokeMethodAsync("OnCloseAnimationComplete");
    return;
  }

  panel.style.overflow = "hidden";
  animateExit(panel, dotNetRef, "OnCloseAnimationComplete");
}

export function dispose(panelId) {
  const inst = instances.get(panelId);
  if (!inst) return;
  if (inst.resizeObserver) {
    inst.resizeObserver.disconnect();
  }
  instances.delete(panelId);
}
