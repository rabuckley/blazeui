---
name: reference-test
description: >-
  Compares BlazeUI styled components against the shadcn/ui reference app using programmatic CSS diff
  and animation behavior verification. Use when the user asks to "compare against reference",
  "visual test", "reference test", "check visual fidelity", "compare to shadcn",
  "screenshot comparison", "visual audit", or "fix animation". Covers starting both apps,
  extracting computed styles, diffing them programmatically, verifying open/close animations
  match the reference, diagnosing root causes (including Blazor render cycle vs animation timing),
  and producing a findings report. Do NOT use for E2E functional testing or unit testing.
allowed-tools: Bash(pnpm:*) Bash(dotnet:*) Bash(curl:*) Bash(lsof:*) Bash(kill:*) Bash(sleep:*) Bash(rm:*) Bash(node:*) Bash(cd:*) Read Edit Write Glob Grep Agent WebFetch
---

# Reference Test

Compares BlazeUI styled components against the shadcn/ui React reference app by extracting all computed CSS styles from both apps and diffing them programmatically.

## Critical rules

- **Match the reference exactly — element types, component decomposition, architecture.** If the reference renders `<div role="dialog">`, BlazeUI must too — not `<dialog>`. If the reference exports 8 composable sub-components, create 8 styled templates — not 1 monolithic wrapper. If Base UI shares Dialog sub-components for AlertDialog (`index.parts.ts` re-exports from `../dialog/`), do the same in BlazeUI. Approximating the reference is not matching it.
- **Verify visually, not just programmatically.** Computed style values can be "correct" while the user sees broken behavior. A `translate` transition playing behind content (wrong `z-index`), an animation with correct frames that completes before the element is visible, or a class that's correct in Tailwind but absent from the compiled CSS — these all pass programmatic checks and fail visually. After any fix, take a screenshot at the relevant viewport size and confirm the result matches expectations.
- **Test the resting state, not just interactive states.** A backdrop that renders on page load (before the dialog has ever been opened), a popup visible at the wrong z-index, or a `display: none` that never gets cleared — these bugs are invisible to tests that only check post-interaction behavior. Always screenshot the page BEFORE interacting.
- **Verify both directions.** Open AND close. Show AND hide. Expand AND collapse. Every reversible interaction has two animations, and they break independently. A close animation can fail while the open animation works perfectly.
- **Run functional tests on overlay components.** After CSS diffs match, write a Playwright script that tests: open via trigger, close via button, close via Escape, close via backdrop click (or verify it does NOT close for alert dialogs), and rapid close→reopen. CSS match does not mean the component works.
- **Test at the viewport size that matters.** Components with responsive behavior (mobile sidebars, collapsible panels, responsive grids) must be tested at the viewport where the behavior activates. Desktop-only testing misses mobile-only code paths entirely.
- **Both sidebars must list the same nav items.** The extractor captures the full page including the sidebar. Mismatched nav items cause element count deltas on every component. Keep `reference/shadcn-app/src/routes/__root.tsx` and `test/BlazeUI.Styled.Demo/Components/Layout/MainLayout.razor` in sync.
- **All CLI commands must run from `tools/reference-test/`** — `pnpm tsx src/cli.ts ...`.
- **Never modify reference app UI components** (`reference/shadcn-app/src/components/ui/*.tsx`). They are scaffolded from shadcn CLI and will be overwritten. Fix issues on the BlazeUI side. Reference route files (`src/routes/*.tsx`) and `__root.tsx` are hand-written and safe to edit.
- **Fix root causes, not symptoms.** When a page has diffs from a shared component (Field, Button, Label), fix the shared styled template — don't work around it on a single demo page. A diff on any page is a diff for every consumer of that component.
- **Treat every reported diff as real until proven otherwise.** `DIFF — N structural` results always need investigation, even when `N` is small.
- **Reproduce the issue before fixing it.** This is a **hard gate**, not a suggestion. Write a short Playwright/Node script that captures the actual broken behavior on both apps before attempting any fix. If you cannot reproduce it programmatically, you do not yet understand it well enough to fix it.

## Diagnosis: read the reference sources first

**Before running any diff tool or attempting any fix**, read ALL of these:

1. **Reference route** (`reference/shadcn-app/src/routes/{component}.tsx`) — demo page structure, wrapper divs, heading text, icons, props, which sub-components compose the page.
2. **Reference styled component** (`reference/shadcn-app/src/components/ui/{component}.tsx`) — exact Tailwind classes, sub-component composition, wrapper elements. **Count the exports** — every exported function is a composable sub-component that needs a BlazeUI styled template equivalent.
3. **Base UI `index.parts.ts`** (`reference/shadcn-app/node_modules/.pnpm/@base-ui+react@*/node_modules/@base-ui/react/esm/{component}/index.parts.ts`) — **check which sub-components are shared from other components.** AlertDialog imports Popup, Backdrop, Close, Trigger, etc. from `../dialog/`. If Base UI shares sub-parts, the BlazeUI headless library should share the same components (via shared context type), not duplicate them.
4. **Base UI primitive source** (`.../@base-ui/react/esm/{component}/`) — HTML tag, data attributes, ARIA roles, inline styles, hidden form inputs, body-level side effects.
5. **Third-party library source** (if applicable) — inline styles, data attributes, JS behaviors.

The diff tool shows *what* differs. The reference source tells you *why* and *where to fix it*.

### What to look for in Base UI source

- **Component sharing via `index.parts.ts`**: AlertDialog re-exports Dialog sub-components. If Base UI shares, BlazeUI should share — don't create duplicate headless component classes.
- **HTML element types**: Check what tag the component renders (`<div>`, `<span>`, `<button>`). Match it exactly. Don't substitute `<dialog>` for `<div role="dialog">` or `<select>` for `<div role="listbox">`.
- **Filtering/state ownership**: Look for hooks like `useFilter`, `useComboboxFilter`. If Base UI handles filtering internally, that logic belongs in the headless layer.
- **Conditional rendering vs CSS visibility**: Check whether sub-components return `null` when inactive. If Base UI conditionally renders, do the same — don't substitute CSS opacity toggling.
- **ARIA attributes on sub-components**: Check for `role`, `aria-live`, `aria-modal` on sub-components. These must be in the headless layer.
- **Mounted vs open state**: Base UI separates `mounted` (element exists in DOM) from `open` (element is logically open). The popup stays `mounted` during close animations. Look for `useOpenChangeComplete` and `useAnimationsFinished`.
- **Responsive/mobile behavior**: Look for `useIsMobile`, `useMediaQuery`. Components like Sidebar render entirely different element trees on mobile.

## Styled template architecture

### Component decomposition must match the reference

The reference `dialog.tsx` exports: `Dialog`, `DialogTrigger`, `DialogContent`, `DialogHeader`, `DialogFooter`, `DialogTitle`, `DialogDescription`, `DialogClose`. BlazeUI must have a corresponding styled template for EACH export. Do NOT create a single monolithic `Dialog.razor` that takes `Title`, `Description`, `ChildContent` as RenderFragments and composes them internally — this prevents consumers from customizing sub-component styling and produces a different DOM structure.

Pattern: simple root wrapper + composable children matching the reference surface.

### Every styled template must declare `Class`

Every styled template that wraps a headless component **must** declare `[Parameter] public string? Class` and merge it via `Css.Cn(baseClasses, Class)`. Without this, consumer `Class` values land in `AdditionalAttributes` and silently override or get dropped.

### Data-attribute variant classes require the attribute to exist

Classes like `data-[size=default]:max-w-xs` do **nothing** unless the element has `data-size="default"`. When porting classes from the reference, check if the reference passes a `data-*` attribute as a prop (e.g. `data-size={size}`). If it does, the styled template must emit the same attribute — either as a parameter default or an explicit attribute on the headless component.

This is a common source of "CSS matches but looks nothing like the reference" bugs. The classes are present in the compiled CSS, but the selector never activates.

## Layer separation

| Concern | Layer | Location |
|---------|-------|----------|
| HTML tag, ARIA roles, inline styles for behavior, `data-` attributes for state | **Headless** | `src/BlazeUI.Headless/Components/` |
| `data-slot`, Tailwind classes, variant enums, visual composition | **Styled template** | `src/BlazeUI.UI.CLI/Templates/` |
| Page structure, example data, which variants to show | **Demo page** | `test/BlazeUI.Styled.Demo/Components/Pages/` |

## Overlay components: close animation lifecycle

Overlay components (dialog, alert-dialog, menu, popover, select, drawer) share a common close animation problem in Blazor Server: Blazor re-renders can reset JS-set inline styles, causing visual flicker.

### The popup

JS `hide()` runs `animateExitCancellable()` which sets `data-closed` and listens for `animationend`. On complete, JS sets `display: none` to prevent the post-animation flash, then calls `OnExitAnimationComplete` back to Blazor.

### The backdrop (sibling element)

The backdrop is a SIBLING of the popup, not managed by the same JS lifecycle. When the dialog closes:

1. Blazor re-renders the backdrop with `data-closed` — this triggers the CSS fade-out animation
2. JS `hide()` onComplete sets `display: none` on both popup AND backdrop (find via `previousElementSibling`)
3. But Blazor may re-render AGAIN (from `OnExitAnimationComplete`), which rebuilds the backdrop DOM, clearing `display: none` and restarting the CSS animation — causing a visible flicker

**Fix:** The backdrop's `BuildRenderTree` should render with `data-closed` exactly ONCE (to trigger the CSS exit animation), then return early on subsequent re-renders. Use a `_closedRendered` flag:

```csharp
if (!Context.Open && _closedRendered) return;  // Already rendered close state
if (!Context.Open) _closedRendered = true;
```

### `data-[closed]:hidden` kills CSS animations

`hidden` sets `display: none` immediately when `data-closed` is applied, preventing the CSS exit animation from playing. Use `data-[closed]:pointer-events-none` instead to let clicks through while the fade-out plays. The actual hiding is done by JS `display: none` in the `animationend` callback.

### Reopen race condition

`hide()` may schedule work (animation listeners, timeouts) that fires after a subsequent `show()`. All overlay JS modules must:
- Use `animateExitCancellable()` (not `animateExit()`) for close animations
- Store the cancellable handle as `inst.pendingClose`
- Cancel pending close + running CSS animations at the top of `show()`
- Cancel pending close in `dispose()`

### Body scroll lock

Dialog and alert-dialog JS must lock body scroll (`overflow: hidden`) on open and unlock on close. Use reference-counted lock/unlock so nested dialogs don't clobber each other.

### Backdrop must not render before first open

The backdrop should only mount after the dialog has been opened at least once. Without a `_mounted` guard (same pattern as the popup), the backdrop renders into the PortalHost immediately, showing a visible overlay on the page before any interaction.

## Blazor ↔ JS attribute ownership

Blazor's `BuildRenderTree` and JS animation code both set attributes on the same DOM elements. When Blazor re-renders, it replaces the full attribute set — any attribute NOT in the render output is removed.

For **non-overlay components** (toggle, checkbox, accordion, tabs), Blazor owns all data attributes.

For **overlay components with CSS animations**, JS must own `data-open`/`data-closed` to avoid race conditions. JS `show()` sets `data-open` after positioning; JS `hide()` sets `data-closed` and listens for `animationend` in the same synchronous block.

**Blazor re-renders also reset inline styles on sibling elements.** If JS sets `display: none` on the backdrop, a Blazor re-render of the backdrop component clears it. This affects any element where JS sets inline styles but Blazor also renders the element.

## Interaction registry

The interaction registry at `tools/reference-test/src/interactions/registry.ts` defines how `batch`, `verify`, and `animate` open/interact with components.

- **Selectors must work on both apps.** Use ARIA attributes (`[aria-haspopup=dialog]`), data-slots (`[data-slot=dialog-close]`), or `:is()` selectors (`":is([role=dialog], dialog[open])"`) — not app-specific attributes.
- **`reverseSelector` must target the actual close mechanism.** `"[role=dialog] button"` picks the FIRST button inside the dialog — which may be a form submit button, not the close button. Use `[data-slot=dialog-close]` or similar specific selectors.
- **`open-pos` check is not meaningful for center-positioned overlays.** Dialogs and alert-dialogs open at viewport center, not near their trigger. The open-pos check compares popup position to anchor position — this always fails for centered overlays. This is expected, not a bug.

## Deciding where a fix belongs

| Symptom | Root cause layer | Fix location |
|---------|-----------------|--------------|
| Wrong wrapper divs, heading text, icons, props | **Demo page** | `test/BlazeUI.Styled.Demo/Components/Pages/` |
| Wrong Tailwind classes, missing sub-components, wrong class composition | **Styled template** | `src/BlazeUI.UI.CLI/Templates/` |
| Wrong HTML tag, missing ARIA attributes, wrong element structure | **Headless component** | `src/BlazeUI.Headless/Components/` |
| Wrong animation timing, missing scroll lock, missing event handlers | **JS module** | `src/BlazeUI.Headless/wwwroot/js/` |

**Demo page fixes are cheap.** Headless component changes are expensive (may break unit tests, E2E tests, and other styled templates). Always check whether a demo page fix is sufficient before touching the headless layer.

### Common mistakes

| Mistake | What to do instead |
|---------|-------------------|
| Monolithic styled template wrapping everything in one file | Create separate styled templates matching each reference export |
| Using a different HTML element than the reference | Match element type exactly (`<div role="dialog">` not `<dialog>`) |
| Passing tests but not taking screenshots | Screenshot resting state + open state + closed state |
| `data-[closed]:hidden` on animated elements | Use `data-[closed]:pointer-events-none`; hide via JS onComplete |
| Duplicating headless sub-components across Dialog/AlertDialog | Check `index.parts.ts` — share via common context type if Base UI shares |
| Data-variant classes with no matching attribute | Verify `data-size`, `data-side`, etc. are actually emitted |
| Removing CSS classes that seem inert | Copy exactly; variants activate when the right data attributes appear |

## Tool: `reftest`

A TypeScript CLI at `tools/reference-test/` that uses Playwright's Node API directly.

### Commands

```bash
cd tools/reference-test

# Batch CSS-only compare (use for initial triage)
pnpm tsx src/cli.ts batch [components...] [--format json|summary|detail]

# Unified verification: CSS diff + animation + behavioral analysis
pnpm tsx src/cli.ts verify [components...] [--format summary|json|detail] [--timeout <ms>]

# Compare open/close animation trajectories
pnpm tsx src/cli.ts animate <component> [--timeout <ms>] [--mode close|switch]

# Extract styles from a single page
pnpm tsx src/cli.ts extract <url> [--blazor] [--interact <component>] [--scope <selector>] [-o <file>]

# Diff two snapshot files
pnpm tsx src/cli.ts diff <ref.json> <test.json> [--format json|summary|detail]

# Dump/diff DOM HTML for a specific element
pnpm tsx src/cli.ts inspect <component> <selector> --diff [--interact <component>]
pnpm tsx src/cli.ts inspect <url> <selector> [--blazor] [--depth 1]
```

All commands accept `--ref-base <url>` and `--test-base <url>` to override the default ports.

## Workflow

### Step 1: Start both apps

```bash
lsof -ti:5199 | xargs kill -9 2>/dev/null
lsof -ti:5200 | xargs kill -9 2>/dev/null

ASPNETCORE_ENVIRONMENT=Development dotnet run --project test/BlazeUI.Styled.Demo --urls http://127.0.0.1:5199 &
cd reference/shadcn-app && pnpm dev --port 5200 &
```

Wait for both to respond 200. Use `http://localhost:5200` (not `127.0.0.1`) for the reference.

### Step 2: Run batch + verify for initial triage

```bash
cd tools/reference-test
pnpm tsx src/cli.ts batch {component} --format detail
pnpm tsx src/cli.ts verify {component} --format detail
```

### Step 3: Run functional tests for overlay components

After CSS diffs are addressed, write a Playwright script that tests all close mechanisms. This is not optional for overlays:

```javascript
// Minimum functional test matrix for overlay components:
// 1. Open via trigger click
// 2. Close via close button (if applicable)
// 3. Close via Escape key
// 4. Close via backdrop click (or verify it stays open for alert dialogs)
// 5. Rapid close→reopen race condition
// 6. Screenshot resting state (before first open — backdrop must not be visible)
// 7. Screenshot open state (compare visually to reference)
// 8. Screenshot closed state (backdrop must not be visible)
```

### Step 4: Fix and verify

**Stale server trap:** Changes are NOT picked up by a running server. After modifying any file, you MUST rebuild and restart before testing.

1. Kill the old server (`lsof -ti:5199 | xargs kill -9`)
2. If styled template changed: re-scaffold + Tailwind rebuild
3. `dotnet build` (with `--no-incremental` if you suspect caching)
4. Start the server and wait for the 200 response
5. Only then run the reftest or Playwright script

After fixing a **styled template**:

```bash
dotnet run --project src/BlazeUI.UI.CLI -- add --all --overwrite --path test/BlazeUI.Styled.Demo
cd test/BlazeUI.Styled.Demo && npx @tailwindcss/cli -i Styles/input.css -o wwwroot/css/app.css --minify
dotnet build test/BlazeUI.Styled.Demo/BlazeUI.Styled.Demo.csproj
lsof -ti:5199 | xargs kill -9 2>/dev/null
ASPNETCORE_ENVIRONMENT=Development dotnet run --project test/BlazeUI.Styled.Demo --urls http://127.0.0.1:5199 &
cd tools/reference-test && pnpm tsx src/cli.ts verify {component}
```

After fixing a **headless component**, skip re-scaffold and Tailwind rebuild — just rebuild + restart. Also run `dotnet test --project test/BlazeUI.Headless.Tests`.

### Step 5: Clean up

```bash
lsof -ti:5199 | xargs kill -9 2>/dev/null
lsof -ti:5200 | xargs kill -9 2>/dev/null
rm /tmp/ref-*.json /tmp/blaze-*.json /tmp/diff-*.json /tmp/ref-*.png /tmp/blaze-*.png 2>/dev/null
```
