// Scroll area JS module. Handles all DOM mutations for the scroll area so that
// Blazor does not need to round-trip the server for high-frequency scroll events.
//
// Responsibilities:
// - Measure thumb size from viewport/content ratio and update CSS custom properties
//   --scroll-area-thumb-height / --scroll-area-thumb-width on the scrollbar element.
// - Translate the thumb via transform: translate3d(...) as the user scrolls.
// - Apply data-scrolling, data-hovering, data-has-overflow-x/y, and overflow-edge
//   data attributes directly on all scrollbar, viewport, and root elements.
// - Update --scroll-area-corner-height / --scroll-area-corner-width CSS vars on
//   the root element once both scrollbars have been measured.
// - Handle thumb dragging (pointerdown / pointermove / pointerup with pointer capture).
// - Handle track clicking (jump to position on click outside the thumb).
// - Observe viewport and content resize via ResizeObserver.
// - Manage tabindex on the viewport: 0 when scrollable, -1 when not.

const SCROLL_TIMEOUT = 500;
const MIN_THUMB_SIZE = 16;

const instances = new Map();

// Register overflow CSS custom properties as non-inheriting so child elements
// don't inherit the viewport's scroll measurements. Matches Base UI's behavior
// where these variables are only meaningful on the viewport element itself.
try {
  const overflowProps = [
    '--scroll-area-overflow-x-start',
    '--scroll-area-overflow-x-end',
    '--scroll-area-overflow-y-start',
    '--scroll-area-overflow-y-end',
  ];
  for (const name of overflowProps) {
    CSS.registerProperty({ name, syntax: '<length>', inherits: false, initialValue: '0px' });
  }
} catch (_) {
  // Property already registered or browser doesn't support CSS.registerProperty.
}

export function init(viewportId, rootId, options) {
  const viewport = document.getElementById(viewportId);
  if (!viewport) return;

  // Clean up any previous instance (e.g., hot reload).
  dispose(viewportId);

  const { overflowEdgeThreshold = 0 } = options ?? {};
  const threshold = {
    xStart: overflowEdgeThreshold,
    xEnd: overflowEdgeThreshold,
    yStart: overflowEdgeThreshold,
    yEnd: overflowEdgeThreshold,
  };

  const inst = { scrollYTimer: null, scrollXTimer: null, resizeObserver: null };
  instances.set(viewportId, inst);

  // Locate companion elements by data-id attributes so we are not reliant on
  // generated element IDs and to support multiple scroll areas on one page.
  // All lookups go through findRoot() which uses the explicit rootId passed at init.
  function findRoot() {
    return document.getElementById(rootId);
  }
  function findScrollbar(orientation) {
    const root = findRoot();
    return root
      ? root.querySelector(`[data-id="${rootId}-scrollbar"][data-orientation="${orientation}"]`)
      : null;
  }
  function findThumb(orientation) {
    const sb = findScrollbar(orientation);
    return sb ? sb.querySelector('[data-orientation]') : null;
  }
  function findCorner() {
    const root = findRoot();
    return root ? root.querySelector(`[data-id="${rootId}-corner"]`) : null;
  }

  // ------------------------------------------------------------------
  // Thumb-position and overflow-edge calculation
  // ------------------------------------------------------------------
  function getOffset(el, property, axis) {
    if (!el) return 0;
    const style = getComputedStyle(el);
    if (property === 'padding') {
      return axis === 'y'
        ? parseFloat(style.paddingTop) + parseFloat(style.paddingBottom)
        : parseFloat(style.paddingLeft) + parseFloat(style.paddingRight);
    }
    // margin
    return axis === 'y'
      ? parseFloat(style.marginTop) + parseFloat(style.marginBottom)
      : parseFloat(style.marginLeft) + parseFloat(style.marginRight);
  }

  function computeThumbPosition() {
    const sbY = findScrollbar('vertical');
    const sbX = findScrollbar('horizontal');
    const thumbY = findThumb('vertical');
    const thumbX = findThumb('horizontal');
    const corner = findCorner();
    const root = findRoot();

    const scrollH = viewport.scrollHeight;
    const scrollW = viewport.scrollWidth;
    const viewH = viewport.clientHeight;
    const viewW = viewport.clientWidth;
    const scrollTop = viewport.scrollTop;
    const scrollLeft = viewport.scrollLeft;

    const hiddenY = viewH >= scrollH;
    const hiddenX = viewW >= scrollW;
    const hiddenCorner = hiddenY || hiddenX;

    // Update tabindex: scrollable regions must be keyboard accessible.
    viewport.tabIndex = hiddenX && hiddenY ? -1 : 0;

    // Update data-has-overflow-x/y on the root and all scrollbar/viewport elements.
    const overflowTargets = [root, sbY, sbX, viewport].filter(Boolean);
    for (const el of overflowTargets) {
      el.toggleAttribute('data-has-overflow-y', !hiddenY);
      el.toggleAttribute('data-has-overflow-x', !hiddenX);
    }

    // Compute scroll-from-start distances for overflow-edge attributes.
    const maxScrollTop = Math.max(0, scrollH - viewH);
    const maxScrollLeft = Math.max(0, scrollW - viewW);
    const scrollTopFromStart = Math.max(0, Math.min(scrollTop, maxScrollTop));
    const scrollTopFromEnd = maxScrollTop - scrollTopFromStart;
    const scrollLeftFromStart = Math.max(0, Math.min(scrollLeft, maxScrollLeft));
    const scrollLeftFromEnd = maxScrollLeft - scrollLeftFromStart;

    const edgeTargets = [root, sbY, sbX, viewport].filter(Boolean);
    for (const el of edgeTargets) {
      el.toggleAttribute('data-overflow-y-start', !hiddenY && scrollTopFromStart > threshold.yStart);
      el.toggleAttribute('data-overflow-y-end', !hiddenY && scrollTopFromEnd > threshold.yEnd);
      el.toggleAttribute('data-overflow-x-start', !hiddenX && scrollLeftFromStart > threshold.xStart);
      el.toggleAttribute('data-overflow-x-end', !hiddenX && scrollLeftFromEnd > threshold.xEnd);
    }

    // Update viewport overflow CSS vars for mask-image fade effects.
    viewport.style.setProperty('--scroll-area-overflow-x-start', `${scrollLeftFromStart}px`);
    viewport.style.setProperty('--scroll-area-overflow-x-end', `${scrollLeftFromEnd}px`);
    viewport.style.setProperty('--scroll-area-overflow-y-start', `${scrollTopFromStart}px`);
    viewport.style.setProperty('--scroll-area-overflow-y-end', `${scrollTopFromEnd}px`);

    // Size and position vertical thumb.
    if (sbY && thumbY && !hiddenY) {
      const sbPaddingY = getOffset(sbY, 'padding', 'y');
      const thumbMarginY = getOffset(thumbY, 'margin', 'y');
      const ratioY = viewH / scrollH;
      const thumbH = Math.max(MIN_THUMB_SIZE, (sbY.offsetHeight - sbPaddingY - thumbMarginY) * ratioY);
      const maxOffsetY = sbY.offsetHeight - thumbH - sbPaddingY - thumbMarginY;
      const scrollRatioY = maxScrollTop === 0 ? 0 : scrollTopFromStart / maxScrollTop;
      const offsetY = Math.min(maxOffsetY, Math.max(0, scrollRatioY * maxOffsetY));
      sbY.style.setProperty('--scroll-area-thumb-height', `${thumbH}px`);
      thumbY.style.transform = `translate3d(0,${offsetY}px,0)`;
    }

    // Size and position horizontal thumb.
    if (sbX && thumbX && !hiddenX) {
      const sbPaddingX = getOffset(sbX, 'padding', 'x');
      const thumbMarginX = getOffset(thumbX, 'margin', 'x');
      const ratioX = viewW / scrollW;
      const thumbW = Math.max(MIN_THUMB_SIZE, (sbX.offsetWidth - sbPaddingX - thumbMarginX) * ratioX);
      const maxOffsetX = sbX.offsetWidth - thumbW - sbPaddingX - thumbMarginX;
      const scrollRatioX = maxScrollLeft === 0 ? 0 : scrollLeftFromStart / maxScrollLeft;
      const offsetX = Math.min(maxOffsetX, Math.max(0, scrollRatioX * maxOffsetX));
      sbX.style.setProperty('--scroll-area-thumb-width', `${thumbW}px`);
      thumbX.style.transform = `translate3d(${offsetX}px,0,0)`;
    }

    // Update corner dimensions on the root CSS vars.
    if (root) {
      const cornerH = !hiddenCorner ? (sbX?.offsetHeight ?? 0) : 0;
      const cornerW = !hiddenCorner ? (sbY?.offsetWidth ?? 0) : 0;
      root.style.setProperty('--scroll-area-corner-height', `${cornerH}px`);
      root.style.setProperty('--scroll-area-corner-width', `${cornerW}px`);
    }

    // Hide corner element when only one (or zero) scrollbars are visible.
    if (corner) {
      if (!hiddenCorner) {
        corner.style.width = `${sbY?.offsetWidth ?? 0}px`;
        corner.style.height = `${sbX?.offsetHeight ?? 0}px`;
        corner.removeAttribute('data-hidden');
      } else {
        corner.style.width = '0px';
        corner.style.height = '0px';
        corner.setAttribute('data-hidden', '');
      }
    }
  }

  // ------------------------------------------------------------------
  // Hover tracking
  // ------------------------------------------------------------------
  function handlePointerEnterOrMove(event) {
    if (event.pointerType !== 'touch') {
      const root = findRoot();
      if (root && root.contains(event.target)) {
        setHovering(true);
      }
    }
  }

  function handlePointerLeave() {
    setHovering(false);
  }

  function setHovering(value) {
    const targets = [findScrollbar('vertical'), findScrollbar('horizontal')].filter(Boolean);
    for (const t of targets) {
      t.toggleAttribute('data-hovering', value);
    }
  }

  // ------------------------------------------------------------------
  // Scroll state (data-scrolling attribute with timeout)
  // ------------------------------------------------------------------
  function setScrollingY(value) {
    clearTimeout(inst.scrollYTimer);
    const targets = [findScrollbar('vertical')].filter(Boolean);
    for (const t of targets) t.toggleAttribute('data-scrolling', value);
    if (value) {
      inst.scrollYTimer = setTimeout(() => setScrollingY(false), SCROLL_TIMEOUT);
    }
  }

  function setScrollingX(value) {
    clearTimeout(inst.scrollXTimer);
    const targets = [findScrollbar('horizontal')].filter(Boolean);
    for (const t of targets) t.toggleAttribute('data-scrolling', value);
    if (value) {
      inst.scrollXTimer = setTimeout(() => setScrollingX(false), SCROLL_TIMEOUT);
    }
  }

  // ------------------------------------------------------------------
  // Scroll event handling
  // ------------------------------------------------------------------
  let lastScrollTop = viewport.scrollTop;
  let lastScrollLeft = viewport.scrollLeft;
  // Track whether the most recent scroll was user-initiated. Programmatic scrolls
  // (e.g., scrollTo() from code) do not set data-scrolling.
  let programmaticScroll = true;

  function onScroll() {
    computeThumbPosition();

    if (!programmaticScroll) {
      const deltaY = viewport.scrollTop - lastScrollTop;
      const deltaX = viewport.scrollLeft - lastScrollLeft;
      if (deltaY !== 0) setScrollingY(true);
      if (deltaX !== 0) setScrollingX(true);
    }

    lastScrollTop = viewport.scrollTop;
    lastScrollLeft = viewport.scrollLeft;

    // Restore programmatic flag after scroll comes to rest.
    clearTimeout(inst.scrollEndTimer);
    inst.scrollEndTimer = setTimeout(() => {
      programmaticScroll = true;
    }, 100);
  }

  function onUserInteraction() {
    programmaticScroll = false;
  }

  viewport.addEventListener('scroll', onScroll);
  viewport.addEventListener('wheel', onUserInteraction, { passive: true });
  viewport.addEventListener('touchmove', onUserInteraction, { passive: true });
  viewport.addEventListener('pointermove', onUserInteraction, { passive: true });
  viewport.addEventListener('keydown', onUserInteraction, { passive: true });

  // ------------------------------------------------------------------
  // Thumb dragging
  // ------------------------------------------------------------------
  // We must use raw addEventListener here (not delegated registry) because these
  // listeners are attached in response to a pointerdown — they are created after the
  // SSR → interactive handoff and the DOM is already stable at that point.

  function setupThumbDrag(orientation) {
    const isVertical = orientation === 'vertical';

    document.addEventListener('pointerdown', (e) => {
      if (e.button !== 0) return;

      const thumb = findThumb(orientation);
      if (!thumb || !e.composedPath().includes(thumb)) return;

      const startPos = isVertical ? e.clientY : e.clientX;
      const startScroll = isVertical ? viewport.scrollTop : viewport.scrollLeft;
      thumb.setPointerCapture(e.pointerId);

      function onPointerMove(ev) {
        const delta = isVertical ? ev.clientY - startPos : ev.clientX - startPos;
        const sb = findScrollbar(orientation);
        if (!sb || !viewport) return;

        const trackSize = isVertical ? sb.offsetHeight : sb.offsetWidth;
        const contentSize = isVertical ? viewport.scrollHeight : viewport.scrollWidth;
        const viewSize = isVertical ? viewport.clientHeight : viewport.clientWidth;
        const scrollRange = contentSize - viewSize;

        const scrollDelta = (delta / trackSize) * scrollRange;
        if (isVertical) {
          viewport.scrollTop = startScroll + scrollDelta;
        } else {
          viewport.scrollLeft = startScroll + scrollDelta;
        }

        ev.preventDefault();
        onUserInteraction();
        if (isVertical) setScrollingY(true);
        else setScrollingX(true);
      }

      function onPointerUp(ev) {
        thumb.releasePointerCapture(ev.pointerId);
        document.removeEventListener('pointermove', onPointerMove);
        document.removeEventListener('pointerup', onPointerUp);
        if (isVertical) setScrollingY(false);
        else setScrollingX(false);
      }

      document.addEventListener('pointermove', onPointerMove);
      document.addEventListener('pointerup', onPointerUp);
    }, { capture: true });
  }

  // ------------------------------------------------------------------
  // Track clicking (jump scroll on click outside thumb)
  // ------------------------------------------------------------------
  function setupTrackClick(orientation) {
    const isVertical = orientation === 'vertical';

    document.addEventListener('pointerdown', (e) => {
      if (e.button !== 0) return;
      const sb = findScrollbar(orientation);
      if (!sb || !e.composedPath().includes(sb)) return;

      const thumb = findThumb(orientation);
      // Thumb has its own drag handler; ignore clicks that land on it.
      if (thumb && e.composedPath().includes(thumb)) return;

      const rect = sb.getBoundingClientRect();
      const sbPadding = getOffset(sb, 'padding', isVertical ? 'y' : 'x');
      const thumbSize = isVertical ? (thumb?.offsetHeight ?? 0) : (thumb?.offsetWidth ?? 0);
      const thumbMargin = getOffset(thumb, 'margin', isVertical ? 'y' : 'x');
      const clickPos = (isVertical ? e.clientY - rect.top : e.clientX - rect.left) - thumbSize / 2 - sbPadding + thumbMargin / 2;
      const trackSize = isVertical ? sb.offsetHeight - sbPadding - thumbMargin : sb.offsetWidth - sbPadding - thumbMargin;
      const maxThumbOffset = trackSize - thumbSize;
      const scrollRatio = maxThumbOffset === 0 ? 0 : Math.max(0, Math.min(clickPos / maxThumbOffset, 1));

      const contentSize = isVertical ? viewport.scrollHeight : viewport.scrollWidth;
      const viewSize = isVertical ? viewport.clientHeight : viewport.clientWidth;
      const newScroll = scrollRatio * (contentSize - viewSize);

      if (isVertical) viewport.scrollTop = newScroll;
      else viewport.scrollLeft = newScroll;

      onUserInteraction();
    }, { capture: true });
  }

  // ------------------------------------------------------------------
  // Wheel on scrollbar (forward to viewport without body scroll)
  // ------------------------------------------------------------------
  function setupScrollbarWheel(orientation) {
    const isVertical = orientation === 'vertical';
    document.addEventListener('wheel', (e) => {
      const sb = findScrollbar(orientation);
      if (!sb || !e.composedPath().includes(sb)) return;
      if (e.ctrlKey) return;

      e.preventDefault();
      if (isVertical) {
        viewport.scrollTop += e.deltaY;
      } else {
        viewport.scrollLeft += e.deltaX;
      }
      onUserInteraction();
    }, { passive: false, capture: true });
  }

  // ------------------------------------------------------------------
  // Hover events (pointer enter / leave on root element)
  // ------------------------------------------------------------------
  function setupHoverTracking() {
    const root = findRoot();
    if (!root) return;
    root.addEventListener('pointerenter', handlePointerEnterOrMove, { passive: true });
    root.addEventListener('pointermove', handlePointerEnterOrMove, { passive: true });
    root.addEventListener('pointerleave', handlePointerLeave, { passive: true });
    // Touch modality check: touch pointers must not set hovering.
    root.addEventListener('pointerdown', (e) => {
      if (e.pointerType === 'touch') setHovering(false);
    }, { passive: true });
  }

  setupThumbDrag('vertical');
  setupThumbDrag('horizontal');
  setupTrackClick('vertical');
  setupTrackClick('horizontal');
  setupScrollbarWheel('vertical');
  setupScrollbarWheel('horizontal');
  setupHoverTracking();

  // ------------------------------------------------------------------
  // ResizeObserver: recompute on viewport and content resize
  // ------------------------------------------------------------------
  inst.resizeObserver = new ResizeObserver(() => {
    computeThumbPosition();
  });
  inst.resizeObserver.observe(viewport);
  // Observe the first child (content element) if present.
  if (viewport.firstElementChild) {
    inst.resizeObserver.observe(viewport.firstElementChild);
  }

  // Initial computation after all child elements are mounted.
  queueMicrotask(computeThumbPosition);

  inst.cleanup = () => {
    viewport.removeEventListener('scroll', onScroll);
    viewport.removeEventListener('wheel', onUserInteraction);
    viewport.removeEventListener('touchmove', onUserInteraction);
    viewport.removeEventListener('pointermove', onUserInteraction);
    viewport.removeEventListener('keydown', onUserInteraction);
    clearTimeout(inst.scrollYTimer);
    clearTimeout(inst.scrollXTimer);
    clearTimeout(inst.scrollEndTimer);
    inst.resizeObserver?.disconnect();
  };
}

export function dispose(viewportId) {
  const inst = instances.get(viewportId);
  if (!inst) return;
  inst.cleanup?.();
  instances.delete(viewportId);
}
