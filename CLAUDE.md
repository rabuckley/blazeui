This project is a set of two Blazor libraries heavily inspired by

- BaseUI (https://base-ui.com/llms.txt, https://github.com/mui/base-ui) - headless
- shadcn/ui (https://ui.shadcn.com/llms.txt, https://github.com/shadcn-ui/ui) - styled wrapper

### Scope

The headless library (`BlazeUI.Headless`) re-implements Base UI's component behavior in Blazor. The styled layer (`BlazeUI.UI.CLI` templates) mirrors shadcn/ui's Tailwind-based styling. Some shadcn components wrap third-party React libraries (e.g. react-day-picker for Calendar, cmdk for Command) that have no Blazor equivalent — these are out of scope. Components composed entirely from other BlazeUI headless primitives (e.g. Sidebar) are in scope even when shadcn's version isn't a direct Base UI wrapper.

### Adding a new test page

1. **Server host**: add the page to `test/BlazeUI.E2E.Host.Server/Components/Pages/`. No `@rendermode` directive — global interactivity is set in `App.razor`.
2. **Wasm and Auto hosts**: add the page to the **`.Client` project** (`Pages/` folder), not the server project. Pages must be in the client assembly so the WASM runtime can resolve them. No `@rendermode` directive needed.
3. Add matching `data-testid` attributes for Playwright selectors.

### Render mode gotchas

- All three hosts use **global interactivity** (`@rendermode` on `<Routes>` in `App.razor`). Don't add per-page `@rendermode` directives.
- For Wasm/Auto hosts, `Routes.razor`, `MainLayout.razor`, and all pages live in the `.Client` project. The WASM runtime can only execute code from assemblies it can download — the server project's assembly is not available on the client.
- `AddBlazeUI()` must be called in **both** server and `.Client` `Program.cs` files. The server registration covers prerendering; the client one covers the actual WASM execution after the interactive handoff.
- **Auto mode note**: each test creates a fresh browser context (no shared cache), so Auto tests always take the Server code path rather than the WASM-on-repeat-visit path. This is fine — both runtimes are covered independently by the Server and WebAssembly hosts. The Auto host validates that the Auto project wiring (dual registration, assembly resolution) works correctly.

### Component reuse in loops

Blazor matches components by position in the render tree, not by identity. In a `@for` loop without `@key`, adding an item at the start reuses the existing component instance at index 0 — its fields, `ElementReference`, JS module, and `OnAfterRenderAsync(firstRender)` state are retained but now bound to a different data item. This silently breaks any component that caches per-instance state (element IDs, measured heights, JS event handlers).

Always use `@key` when rendering components in a loop where items can be added, removed, or reordered:

```razor
@for (var i = 0; i < items.Count; i++)
{
    <MyComponent @key="items[i].Id" Data="items[i]" />
}
```

Without `@key`, `firstRender` only fires once per position — if you prepend a new item, the component at position 0 doesn't re-run `OnAfterRenderAsync(firstRender: true)` because Blazor considers it the same component instance that already rendered.

## Styled Demo App

`test/BlazeUI.Styled.Demo/` is a Blazor Server app that renders all styled components with Tailwind v4 for visual verification.

### Re-scaffolding components

The styled demo uses CLI-scaffolded components in `test/BlazeUI.Styled.Demo/Components/UI/`. To update all components after template changes:

```bash
dotnet run --project src/BlazeUI.UI.CLI -- add --all --overwrite --path test/BlazeUI.Styled.Demo
```

### Running the demo

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project test/BlazeUI.Styled.Demo
```

## Overlay Component Architecture

### Portal system

Overlay components (Dialog, Menu, Popover, etc.) use a Portal system to render popups in a `PortalHost` at the top of the DOM. This requires:

1. **`<PortalHost />`** in every host app's `MainLayout.razor` (with `@using BlazeUI.Headless.Overlay`).
2. Each `*Portal` component **re-cascades its parent context** so children rendered through `PortalHost` retain access to the Root's cascading value.
3. `Portal.OnParametersSet` calls `PortalService.Update()` on every parent re-render to trigger `PortalHost` re-render and propagate state changes.

### `data-testid` on Root components

Root components that extend `ComponentBase` directly (all overlay Roots: `DialogRoot`, `MenuRoot`, `PopoverRoot`, etc.) do **not** accept unmatched HTML attributes. Never add `data-testid` to them in test pages — it causes a runtime `InvalidOperationException`. Only `BlazeElement`-derived components (e.g., `ToggleRoot`, `SwitchRoot`, `ProgressRoot`) support `data-testid`.

### show()/hide() lifecycle

- Popup components that render their own `<dialog>` or popup element enqueue `ShowPopoverMutation` / `HidePopoverMutation` onto a scoped `BrowserMutationQueue` (from `BlazeUI.Bridge`) during `OnParametersSet`, then call `MutationQueue.FlushAsync()` in `OnAfterRenderAsync`. The queue deduplicates by `ElementId` (last-write-wins) so rapid open/close toggles collapse to the final state. For `DefaultOpen`, the **Root** (not the Popup) enqueues the initial show mutation in `OnJsInitializedAsync` — at that point `JsModule` is guaranteed set and `FlushAsync` runs immediately after. Popup components only enqueue on subsequent open transitions. When adding new overlay popups, follow this same pattern — never call JS `show()`/`hide()` directly from component lifecycle methods.
- Menu/Popover/ContextMenu/Tooltip: JS `hide()` is called directly from the Root's `SetOpenAsync(false)` method, bypassing the Portal re-render chain. This is necessary because `CascadingValue` uses reference equality — mutating a context object's properties doesn't trigger re-cascading to Positioner children in Blazor Server mode.

### JS event delegation for SSR → interactive handoff

Blazor Server's SSR → interactive handoff replaces DOM elements. Event listeners attached to specific elements during `OnAfterRenderAsync(firstRender: true)` end up on detached nodes. Component JS that needs persistent event listeners must use `registerDelegatedHandler` / `unregisterDelegatedHandler` from `/_content/BlazeUI.Bridge/blazeui.bridge.js`. The registry maintains one `document`-level listener per event type and dispatches to handlers by `getElementById` + `contains` check.

**Decision rule — registry vs raw `addEventListener`:**
- **Registry (persistent):** Listeners registered during `init()` / `firstRender` that must survive the SSR → interactive DOM replacement. These live for the component's lifetime.
- **Raw `addEventListener` (ephemeral):** Listeners attached during user interaction (e.g., `pointermove` during a drag, `escape` key during a dialog show, `pointerdown` for click-outside). These are created after the SSR handoff when the DOM is stable, so delegation is unnecessary.

### First-open positioning race (floating popups)

Floating-ui's `computePosition` is async (returns a Promise). On first open, `show()` is called from `OnAfterRenderAsync` after Blazor renders the popup into the DOM. If `data-open` is set (triggering the CSS entry animation) before the async position resolves, the popup animates from its default position (0, 0) instead of from the anchor. Subsequent opens don't have this problem because the element retains its `left`/`top` from the previous open.

Three things conspire to make this visible:

1. **Blazor renders before JS runs.** The popup element exists in the DOM at its default position before JS `show()` executes. If Blazor's render sets `data-open`, the CSS animation starts immediately from the wrong position.
2. **`position()` is async.** Even when awaited, the microtask boundary gives Blazor a window to re-render and overwrite JS-set inline styles.
3. **`duration-100` creates CSS transitions on all properties.** When `startAutoUpdate` corrects the position from (0, 0) to the anchor coordinates, the `left`/`top` change is visibly transitioned instead of snapping.

**The fix has three parts:**

- **Headless popup components** (`MenuPopup`, `PopoverPopup`, `SelectPopup`): do NOT render `data-open`/`data-closed` — JS exclusively owns these attributes.
- **Styled templates**: add `opacity-0 data-open:opacity-100 data-closed:opacity-100` so the popup is invisible before JS positions it, visible when JS sets `data-open`, and remains visible during close animations (`data-closed`).
- **JS `show()`**: set `opacity: 0` and `transition: none` before `showPopover()`, call `startAutoUpdate` (which fires `position()` immediately), then defer the reveal (`data-open`, `animateEntry`) to the next `requestAnimationFrame`. By the next frame the async `position()` microtask has resolved, `left`/`top` are correct, and the animation starts from the anchor.

```javascript
// Pattern for positioned overlay show():
popup.style.opacity = "0";
popup.style.transition = "none";
popup.setAttribute("popover", "manual");
showPopover(popup);

inst.autoUpdateId = startAutoUpdate(trigger, popup, options);

requestAnimationFrame(() => {
  popup.offsetHeight;            // flush position
  popup.style.transition = "";   // re-enable transitions
  popup.style.opacity = "";      // clear inline override
  popup.setAttribute("data-open", "");
  animateEntry(popup);
});
```

### JS Escape key handlers

Escape key handlers (`onEscapeKey`) must be registered on `document`, not on the popup element. Blazor's interactive handoff completes before `requestAnimationFrame` focus runs, so focus may still be on the trigger when Playwright sends the Escape keypress. A popup-scoped listener misses the event.

### Exit animation lifecycle

Overlay close animations require coordination between Blazor (which owns `data-closed`) and JS (which must keep the element visible until the CSS animation finishes). The pattern, used by Dialog, AlertDialog, and Drawer:

1. **Use `animateExitCancellable`**, not `animateExit`. Store the handle as `inst.pendingClose` so `show()` can cancel a pending close for rapid reopen.
2. **JS `hide()` sets `display: none`** on both the popup and its backdrop sibling inside `onComplete` — not before, not in Blazor.
3. **JS `show()` clears `display: none`** on both elements, cancels `pendingClose`, and cancels any running CSS animations via `el.getAnimations().forEach(a => a.cancel())`.
4. **`OnExitAnimationComplete`** (the Blazor callback) fires inside `onComplete`, so the Blazor re-render happens after `display: none` is set. This means Blazor's diff sees unchanged attributes and preserves the JS-set style.

### CSS classes that break exit animations

Two Tailwind patterns silently kill CSS exit animations when applied via `data-[closed]:`:

- **`data-[closed]:hidden`** — sets `display: none` immediately when Blazor renders `data-closed`, before the CSS animation can start. Use `data-[closed]:pointer-events-none` instead; JS handles the actual hiding in `onComplete`.
- **`data-[closed]:opacity-0`** with an implicit-`from` keyframe — if `@keyframes fadeOut { to { opacity: 0 } }` has no explicit `from`, the browser uses the underlying computed value as the start. When `opacity-0` is applied at the same instant (via `data-[closed]`), the start value is already 0, so the animation interpolates 0→0 (invisible). Fix: always use explicit `from { opacity: 1 }` in exit keyframes, and use `animation-fill-mode: forwards` to hold the final state instead of a static class fallback.

### Backdrop `_mounted` guard

Overlay backdrops (`DrawerBackdrop`, `DialogBackdrop`) must not render before the overlay has been opened at least once. Without a `_mounted` guard in `BuildRenderTree`, the backdrop renders into the `PortalHost` on initial load, producing a visible overlay before any interaction. Set `_mounted = true` when the context signals open, and return early from `BuildRenderTree` while `!_mounted`.
