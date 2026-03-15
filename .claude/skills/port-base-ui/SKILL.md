---
name: port-base-ui
description: >-
  Ports a complete headless UI component from Base UI (React) to BlazeUI.Headless (Blazor).
  Use when the user asks to "port [Component] from Base UI", "implement [Component] headless",
  "add [Component] to BlazeUI.Headless", or wants to bring an entire Base UI component
  (with all its sub-parts) into the Blazor headless library. Covers fetching Base UI source
  and tests from GitHub, planning the Blazor file structure, implementing all sub-parts,
  writing bUnit tests, and verifying the build.
  Do NOT use for styled/template components (use port-component instead), fixing existing
  components, adding individual sub-parts to an already-ported component, or general code review.
allowed-tools: Bash(dotnet:*) Read Edit Write Glob Grep Agent WebFetch
---

# Port Base UI Component

Ports a complete headless component from Base UI's React source to BlazeUI.Headless in Blazor.

## Critical: Base UI Is the Source of Truth

Every implementation decision must be grounded in the actual Base UI source code. Do not guess at behavior, invent ARIA attributes, or assume DOM structure. Fetch and read the React implementation and tests before writing any Blazor code. When in doubt, re-read the Base UI source.

## Workflow

### Step 1: Fetch and read the Base UI source

Fetch the component directory listing from GitHub to discover all sub-parts:

```
WebFetch https://api.github.com/repos/mui/base-ui/contents/packages/react/src/{component-name}
```

Then fetch the implementation file for every sub-part directory found. The typical structure is:

```
packages/react/src/{component-name}/
  root/             — Root state container
  trigger/          — Interactive trigger element
  popup/            — Floating popup content
  positioner/       — Floating-ui positioning wrapper
  portal/           — React portal
  backdrop/         — Overlay backdrop
  arrow/            — Popup arrow element
  ...               — Component-specific sub-parts
  index.parts.ts    — Canonical list of all exported sub-parts
```

For each sub-part, fetch and read the main `.tsx` file:

```
WebFetch https://raw.githubusercontent.com/mui/base-ui/master/packages/react/src/{component-name}/{sub-part}/{PascalName}.tsx
```

Also fetch any context files (`*Context.ts`), data attribute enums (`*DataAttributes.ts`), and the root's hook file if it contains core logic.

**Critical: Understanding Base UI's default state-to-data-attribute mapping**

Base UI's `getStateAttributesProps` (in `packages/react/src/utils/getStateAttributesProps.ts`) has an **implicit default mapping**: for any state property that is truthy, it automatically emits `data-{key}=""`. For example, if a component's state is `{ disabled: true, open: false }`, the rendered element gets `data-disabled=""` (truthy) but no `data-open` (falsy). This happens **even when no explicit `*DataAttributes.ts` file exists** for that component.

When auditing data attributes:
1. Check the `*DataAttributes.ts` file if it exists — it may define **additional** custom-mapped attributes
2. Also check the **state shape** (`*State` interface) — every truthy state field produces a `data-*` attribute via the default mapping
3. Check for `stateAttributesMapping` overrides in the component's `useRenderElement` call — custom mappings (like `fieldValidityMapping`, `popupStateMapping`, `transitionStatusMapping`) replace the default for specific fields
4. The absence of a `*DataAttributes.ts` file does **not** mean "no data attributes" — it means the component uses only the default mapping from its state shape

Native HTML attributes (like `disabled` on `<fieldset>` or `<button>`) pass through via `...elementProps` spread in `useRenderElement` and are **not** controlled by the state-to-data-attribute mapping.

**Extract from each sub-part:**
- Default HTML element tag (what `useRenderElement` renders)
- ARIA attributes (roles, `aria-expanded`, `aria-controls`, `aria-labelledby`, etc.)
- Data attributes (from `*DataAttributes.ts` **and** the default state mapping — check both)
- State shape (what the component's state object contains — every truthy field becomes a `data-*` attribute)
- Context dependencies (what it reads from parent contexts)
- Event handling (click, keyboard, hover-intent, pointer events)
- JS-heavy behavior (positioning, pointer lock, scroll detection, focus trapping)

### Step 2: Fetch and read the Base UI tests

Look for test files in the component directory and in the shared test directory:

```
WebFetch https://api.github.com/repos/mui/base-ui/contents/packages/react/src/{component-name}/root
```

Test files are named `*.test.tsx` and live alongside the sub-part implementation. Also check:

```
WebFetch https://api.github.com/repos/mui/base-ui/contents/packages/react/test
```

From each test file, identify high-value tests to port:
- **Port**: ARIA contract tests, keyboard navigation, state transitions, disabled/readonly behavior, accessibility assertions
- **Skip**: React-specific tests (ref forwarding, hook internals, render prop composition, `React.forwardRef` behavior)

### Step 3: Read existing BlazeUI patterns

Before writing code, read the codebase to understand established patterns:

1. **Read CLAUDE.md and SESSION.md** for architectural context and known issues
2. **Read an analogous existing component** — pick one that matches the new component's category:
   - Overlay with popup: read `Dialog` or `Popover` components
   - Inline interactive: read `Toggle` or `Checkbox` components
   - Container with items: read `Accordion` or `Tabs` components
   - Form control: read `NumberField` or `Select` components
3. **Read the base classes** you'll extend:
   - `src/BlazeUI.Headless/Core/BlazeElement.cs` for DOM-rendering components
   - `src/BlazeUI.Headless/Core/OverlayRoot.cs` for overlay root components
   - `src/BlazeUI.Headless/Core/ComponentState.cs` for controlled/uncontrolled state
4. **Read existing test files** in `test/BlazeUI.Headless.Tests/Components/` for test patterns

### Step 4: Plan the Blazor file structure

Before coding, produce a plan with one file per line. Map every Base UI sub-part to a BlazeUI file:

```
src/BlazeUI.Headless/Components/{Component}/
  {Component}Root.cs           — extends OverlayRoot or ComponentBase
  {Component}Context.cs        — internal sealed class, cascaded to children
  {Component}Trigger.cs        — extends BlazeElement, renders button
  {Component}Popup.cs          — extends BlazeElement, mounted/unmounted
  ...

src/BlazeUI.Headless/wwwroot/js/{component}/
  {component}.js               — JS module (only if component needs JS interop)

test/BlazeUI.Headless.Tests/Components/{Component}/
  {Component}Tests.cs          — bUnit tests
```

For each file, note:
- Which Base UI file it corresponds to
- What base class it extends
- Key ARIA attributes it must emit
- Whether it needs JS interop

Share this plan with the user before proceeding.

### Step 5: Implement the context class

Create the internal context class first — every other component depends on it.

Follow this pattern exactly:

```csharp
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.{Component};

internal sealed class {Component}Context
{
    // Open/active state
    public bool Open { get; set; }

    // Element IDs for ARIA wiring
    public string TriggerId { get; set; } = "";
    public string PopupId { get; set; } = "";

    // Delegates back to Root
    public Func<bool, Task> SetOpen { get; set; } = _ => Task.CompletedTask;

    // JS module (shared with children that need interop)
    public IJSObjectReference? JsModule { get; set; }
    public DotNetObjectReference<{Component}Root>? DotNetRef { get; set; }
}
```

Only add properties that the Base UI context actually provides. Do not invent properties.

### Step 6: Implement the root component

The root manages state, owns the JS module lifecycle, and cascades context. Choose the correct base class:

| Component type | Base class | Examples |
|---|---|---|
| Overlay with popup | `OverlayRoot` | Dialog, Menu, Popover, Select, Tooltip |
| Inline interactive | `BlazeElement` | Toggle, Switch, Checkbox, NumberField |
| State-only container | `ComponentBase` | Accordion, Tabs, NavigationMenu |

For `OverlayRoot` subclasses, implement the abstract contract:
- `ModulePath` — path to JS module
- `JsInstanceKey` — unique instance key for JS
- `SyncContextState()` — push open state onto context
- `OnJsInitializedAsync()` — create DotNetRef, push JS refs onto context
- `OnDispose()` — dispose DotNetRef
- `OnBeforeCloseAsync()` — enqueue hide mutation (if applicable)
- `[JSInvokable]` callback methods matching JS event handlers

For the constructor, generate element IDs via `IdGenerator.Next(...)` and assign delegate wiring.

### Step 7: Implement child components

Each child component is a `BlazeElement` subclass that:

1. Receives context via `[CascadingParameter] internal {Component}Context Context { get; set; } = default!;`
2. Defines `DefaultTag` matching the Base UI default element
3. Returns a state record from `GetCurrentState()`
4. Yields data attributes from `GetDataAttributes()` — match Base UI's default state mapping **and** any `*DataAttributes.ts` overrides (see "Understanding Base UI's default state-to-data-attribute mapping" above)
5. Yields ARIA and event attributes from `GetExtraAttributes()`

State records are `readonly record struct` with the minimum fields needed:

```csharp
public readonly record struct {Component}TriggerState(bool Open);
```

### Step 8: Implement JS module (if needed)

Only create a JS module if the component requires browser APIs that Blazor cannot access (positioning, pointer lock, scroll detection, focus trapping, hover-intent, keyboard navigation).

Place it at `src/BlazeUI.Headless/wwwroot/js/{component}/{component}.js`.

Follow the established pattern:
- Import utilities from `/_content/BlazeUI.Headless/blazeui.core.js`
- Use a `Map` keyed by instance ID for per-instance state
- Export `init()`, action functions, and `dispose(instanceKey)`
- Handle cleanup in `dispose()` (remove listeners, clear timers, delete from Map)

### Step 9: Write bUnit tests

Create `test/BlazeUI.Headless.Tests/Components/{Component}/{Component}Tests.cs`.

Port high-value tests from Base UI's test files. Translate React Testing Library patterns to bUnit:

| React Testing Library | bUnit equivalent |
|---|---|
| `screen.getByRole('button')` | `cut.Find("button")` or `cut.Find("[role='button']")` |
| `expect(el).toHaveAttribute('aria-expanded', 'true')` | `Assert.Equal("true", el.GetAttribute("aria-expanded"))` |
| `fireEvent.click(el)` | `el.Click()` |
| `expect(el).toBeInTheDocument()` | `Assert.NotNull(cut.Find(...))` |
| `expect(el).not.toBeInTheDocument()` | `Assert.Empty(cut.FindAll(...))` |
| `screen.queryByText('...')` | `Assert.Contains("...", cut.Markup)` |

Test structure:

```csharp
using BlazeUI.Headless.Components.{Component};
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.{Component};

public class {Component}Tests : BunitContext
{
    public {Component}Tests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    // For components that can be tested via context injection:
    private {Component}Context CreateContext(...) => new() { ... };

    [Fact]
    public void Trigger_HasCorrectAriaAttributes()
    {
        // Arrange
        var ctx = CreateContext(open: false);

        // Act
        var cut = Render(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<{Component}Trigger>(...));

        // Assert
        Assert.Equal("false", cut.Find("button").GetAttribute("aria-expanded"));
    }
}
```

**Prioritize these test categories (in order):**
1. ARIA attribute correctness (roles, expanded, controls, labelledby)
2. Data attribute presence/absence based on state
3. State transitions (click toggles, selection changes)
4. Disabled/readonly behavior (prevents interaction, sets attributes)
5. Keyboard navigation (if testable without full JS)

### Step 10: Build and verify

```bash
# Build the headless library
dotnet build src/BlazeUI.Headless/BlazeUI.Headless.csproj

# Run unit tests (must include all existing + new tests)
dotnet test --project test/BlazeUI.Headless.Tests/BlazeUI.Headless.Tests.csproj

# Build the full solution to catch downstream breakage
dotnet build
```

All existing tests must continue to pass. Fix any compilation errors or test failures before proceeding.

### Step 11: Update SESSION.md

Add notes for any:
- Deferred sub-parts or behaviors (with reason)
- Known limitations (e.g., "swipe gestures impractical in Server mode")
- JS budget concerns (current bundle is ~8KB gzipped)
- Differences from Base UI's architecture (with justification)

Remove any items from SESSION.md that this port resolves.

## Component Classification Reference

| Base UI Component | Root Base Class | Needs JS? | Needs Portal? | Key Sub-parts |
|---|---|---|---|---|
| Accordion | ComponentBase | No | No | Root, Item, Trigger, Panel |
| AlertDialog | OverlayRoot | Yes | Yes | Root, Trigger, Popup, Title, Description, Close |
| Checkbox | BlazeElement | No | No | Root, Indicator |
| Collapsible | ComponentBase | No | No | Root, Trigger, Panel |
| Dialog | OverlayRoot | Yes | Yes | Root, Trigger, Popup, Backdrop, Title, Description, Close |
| Drawer | OverlayRoot | Yes | Yes | Root, Trigger, Popup, Backdrop, Close, Title, Description |
| Menu | OverlayRoot | Yes | Yes | Root, Trigger, Popup, Positioner, Item, Separator, Group |
| NumberField | BlazeElement | Yes | No | Root, Input, Increment, Decrement, Group, ScrubArea |
| Popover | OverlayRoot | Yes | Yes | Root, Trigger, Popup, Positioner, Arrow, Backdrop |
| Select | OverlayRoot | Yes | Yes | Root, Trigger, Value, Popup, Positioner, Item, Group |
| Slider | BlazeElement | Yes | No | Root, Track, Thumb, Output |
| Switch | BlazeElement | No | No | Root, Thumb |
| Tabs | ComponentBase | Yes | No | Root, List, Tab, Panel |
| Toggle | BlazeElement | No | No | Root |
| Tooltip | OverlayRoot | Yes | Yes | Root, Trigger, Positioner, Popup, Arrow |

## Troubleshooting

### Context class not accessible from tests
The context class is `internal`. Tests in `BlazeUI.Headless.Tests` can access it because the test project has `InternalsVisibleTo` configured. If you get access errors, check the headless project's `AssemblyInfo.cs` or `.csproj` for the attribute.

### JSInterop calls fail in bUnit tests
Set `JSInterop.Mode = JSRuntimeMode.Loose` in the test constructor. This tells bUnit to accept any JS call without explicit setup. Real JS behavior is verified via E2E tests, not bUnit.

### OverlayRoot subclass doesn't render
`OverlayRoot` has no `BuildRenderTree` — subclasses must override it to cascade their context type via `CascadingValue`.

### ID prefix mismatch in test assertions
`IdGenerator.Next("foo")` produces IDs like `blazeui-foo-123`, not `foo-123`. Use `Assert.Contains("foo-", id)` instead of `Assert.StartsWith("foo-", id)`.

### Existing tests fail after adding new component
The new component's namespace or class name may conflict with an existing one. Check for ambiguous type references and add explicit `using` directives.
