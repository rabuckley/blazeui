import {
  registerDelegatedHandler,
  unregisterDelegatedHandler,
} from "/_content/BlazeUI.Bridge/blazeui.bridge.js";

const instances = new Map();

export function init(rootId, opts, ref) {
  // Clean up any previous instance for this root.
  dispose(rootId);

  const inst = {
    dotNetRef: ref,
    enterTimer: null,
    exitTimer: null,
    options: opts,
  };
  instances.set(rootId, inst);

  // Hover-intent via pointer delegation
  registerDelegatedHandler("pointerover", rootId, (e, root) => {
    const trigger = e.target.closest(`#${rootId} [data-value]`);
    if (!trigger) return;
    const value = trigger.getAttribute("data-value");
    if (!value) return;
    clearTimeout(inst.exitTimer);
    inst.enterTimer = setTimeout(() => {
      ref.invokeMethodAsync("OnHoverEnter", value);
    }, opts.enterDelay ?? 200);
  });

  registerDelegatedHandler("pointerout", rootId, (e, root) => {
    clearTimeout(inst.enterTimer);
    inst.exitTimer = setTimeout(() => {
      ref.invokeMethodAsync("OnHoverExit");
    }, opts.exitDelay ?? 300);
  });

  // Keyboard navigation
  registerDelegatedHandler("keydown", rootId, (e, root) => {
    const items = [...root.querySelectorAll("button[data-value]")];
    if (items.length === 0) return;
    const currentIndex = items.indexOf(document.activeElement);
    if (currentIndex === -1) return;

    const isHorizontal = opts.orientation === "horizontal";
    const nextKey = isHorizontal ? "ArrowRight" : "ArrowDown";
    const prevKey = isHorizontal ? "ArrowLeft" : "ArrowUp";

    switch (e.key) {
      case nextKey: {
        e.preventDefault();
        const next = currentIndex < items.length - 1 ? currentIndex + 1 : 0;
        items[next].focus();
        break;
      }
      case prevKey: {
        e.preventDefault();
        const prev = currentIndex > 0 ? currentIndex - 1 : items.length - 1;
        items[prev].focus();
        break;
      }
      case "Escape": {
        e.preventDefault();
        ref.invokeMethodAsync("OnHoverExit");
        break;
      }
    }
  });
}

export function show(viewportId, contentId) {
  const viewport = document.getElementById(viewportId);
  if (!viewport) return;

  const wasHidden = viewport.hidden;
  viewport.removeAttribute("hidden");
  viewport.removeAttribute("data-ending-style");

  // Measure the active content panel and expose its dimensions as CSS custom
  // properties so the viewport container can animate its size smoothly.
  // Content panels are portaled into the viewport via RenderFragment registration,
  // and we look them up by their element ID passed from C#.
  const content = contentId ? document.getElementById(contentId) : null;

  // Measure the content's natural size. Content panels have `h-full` (height: 100%
  // of the viewport), so scrollHeight returns the viewport's current height, not the
  // content's natural height. Temporarily unset the height constraint to get the
  // intrinsic dimensions.
  let contentW = 0, contentH = 0;
  if (content) {
    const prevH = content.style.height;
    const prevW = content.style.width;
    content.style.height = "auto";
    content.style.width = "max-content";
    contentW = content.scrollWidth;
    contentH = content.scrollHeight;
    content.style.height = prevH;
    content.style.width = prevW;
  }

  // Animate the viewport entry. When the viewport was hidden (display: none),
  // commit the starting state (0 size, transparent, scaled down) before
  // transitioning to the final state. The CSS vars must be set to 0 first,
  // then updated to the measured size so the CSS transition interpolates.
  if (wasHidden) {
    viewport.style.transition = "none";
    viewport.style.setProperty("--popup-width", "0px");
    viewport.style.setProperty("--popup-height", "0px");
    viewport.setAttribute("data-starting-style", "");
    viewport.offsetHeight; // commit starting state

    viewport.style.transition = "";
    viewport.style.setProperty("--popup-width", `${contentW}px`);
    viewport.style.setProperty("--popup-height", `${contentH}px`);
    requestAnimationFrame(() => {
      viewport.removeAttribute("data-starting-style");
    });
  } else {
    // Already visible — just update size. The CSS transition on
    // width/height handles the smooth resize. Don't call animateEntry
    // here — that adds an unwanted opacity/scale bounce on content switches.
    viewport.style.setProperty("--popup-width", `${contentW}px`);
    viewport.style.setProperty("--popup-height", `${contentH}px`);
  }

  // Animate the new content panel entry with a directional slide.
  if (content) {
    content.style.transition = "none";
    content.setAttribute("data-starting-style", "");
    requestAnimationFrame(() => {
      content.style.transition = "";
      requestAnimationFrame(() => {
        content.removeAttribute("data-starting-style");
      });
    });
  }

  // Cancel any pending exit across all instances
  for (const inst of instances.values()) {
    clearTimeout(inst.exitTimer);
  }
}

export function hide(viewportId, ref) {
  const viewport = document.getElementById(viewportId);
  if (!viewport) {
    ref?.invokeMethodAsync("OnExitAnimationComplete");
    return;
  }

  // Set data-ending-style to trigger CSS transition (opacity/scale down).
  // animateExit only calls the .NET callback — it doesn't hide the element.
  // We need to set hidden after the transition completes so the viewport
  // doesn't remain visible at full opacity.
  viewport.setAttribute("data-ending-style", "");

  let done = false;
  const complete = () => {
    if (done) return;
    done = true;
    viewport.removeAttribute("data-ending-style");
    viewport.setAttribute("hidden", "");
    ref?.invokeMethodAsync("OnExitAnimationComplete");
  };

  const timeout = setTimeout(complete, 350);
  viewport.addEventListener("transitionend", (e) => {
    if (e.target === viewport) {
      clearTimeout(timeout);
      complete();
    }
  }, { once: true });
}

export function dispose(rootId) {
  const inst = instances.get(rootId);
  if (!inst) return;
  clearTimeout(inst.enterTimer);
  clearTimeout(inst.exitTimer);
  unregisterDelegatedHandler("pointerover", rootId);
  unregisterDelegatedHandler("pointerout", rootId);
  unregisterDelegatedHandler("keydown", rootId);
  instances.delete(rootId);
}
