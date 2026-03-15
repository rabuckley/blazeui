---
name: port-component
description: >-
  End-to-end workflow for porting a UI component to BlazeUI from BaseUI (headless) and shadcn/ui (styled).
  Use when the user asks to "port a component", "add a new component", "implement [ComponentName]",
  or wants to bring a BaseUI/shadcn component into the library. Covers headless anatomy research,
  styled template creation, demo page, Tailwind CSS compilation, and visual verification via playwright-cli.
  Do NOT use for fixing existing components or general code review.
allowed-tools: Bash(npx:*) Bash(dotnet:*) Bash(sed:*) Bash(playwright-cli:*) Bash(curl:*) Bash(lsof:*) Bash(kill:*) Read Edit Write Glob Grep Agent WebFetch
---

# Port Component

Ports a UI component into BlazeUI by researching BaseUI for headless anatomy and shadcn/ui for styled classes, then implementing and visually verifying the result.

## Important

- Always edit **CLI templates** in `src/BlazeUI.UI.CLI/Templates/`, not compiled components directly.
- Regenerate compiled components after every template change (see Step 5).
- Tailwind v4 does not scan `.razor` files by default. The `@source` directives in `test/BlazeUI.Styled.Demo/Styles/input.css` handle this. If you add classes that don't appear in any `.razor` file already scanned, rebuild CSS and verify.
- BlazeUI uses `data-[open]`/`data-[closed]` and `data-[pressed]`/`data-[active]` attributes (not `data-[state=open]` like Radix/shadcn). Translate attribute selectors accordingly.

## Workflow

### Step 0: Determine the component type

Not all components follow the same path. Before starting, decide which category the component falls into:

| Category | Has BaseUI equivalent? | Has headless layer? | Examples |
|----------|----------------------|-------------------|----------|
| **Standard** | Yes | Yes — wraps `BlazeUI.Headless.Components.*` | Button, Dialog, Accordion, Tabs, Menu, Select |
| **Styled-only** | No | No — pure layout/composition pattern | Sidebar, Card, Badge, Breadcrumb |

**Standard components** follow Steps 1–10 below in order.

**Styled-only components** skip Steps 1 and 3 (no headless anatomy or attribute translation needed). They differ in several important ways:

- **No headless layer**: All logic lives in the styled templates. State management uses `CascadingValue` with a context class (e.g. `SidebarContext.cs`) created directly in `src/BlazeUI.UI/Components/`.
- **Multiple template files**: Complex styled-only components (like Sidebar) decompose into many sub-components (Provider, Header, Content, Menu, MenuItem, etc.), each in its own `.razor` template file. Name them with a shared prefix (e.g. `SidebarProvider.razor`, `SidebarMenu.razor`).
- **Theme tokens**: Components with their own color scheme (like Sidebar) need custom CSS tokens added to the demo app's `Styles/input.css` — both the `@layer base` variables and the `@theme` mappings.
- **No attribute translation**: Since there's no Radix/BaseUI layer, the component defines its own data attributes directly (e.g. `data-state`, `data-collapsible`, `data-variant`).
- **Demo app integration**: Layout components often need to be wired into `MainLayout.razor` rather than just having a standalone demo page.

For styled-only components, start at Step 2 (research shadcn classes), then jump to Step 4 (create templates).

### Step 1: Research the headless anatomy from BaseUI

> Skip this step for styled-only components.

Fetch the BaseUI documentation for the component to understand the headless parts (root, trigger, content, etc.), their roles, ARIA attributes, and keyboard interactions.

```
WebFetch https://base-ui.com/llms.txt
```

Look for the specific component section. Extract:
- **Part list**: every sub-component (e.g. `Root`, `Trigger`, `Popup`, `Arrow`)
- **State attributes**: what data attributes are emitted (`data-open`, `data-pressed`, etc.)
- **Keyboard behavior**: arrow keys, Escape, Enter, Tab
- **ARIA roles**: `role`, `aria-expanded`, `aria-controls`, etc.

Cross-reference with the existing headless components in `src/BlazeUI.Headless/Components/` to see what already exists and what gaps remain.

### Step 2: Research the styled classes from shadcn/ui

Fetch the shadcn/ui v4 component source from GitHub:

```
WebFetch https://raw.githubusercontent.com/shadcn-ui/ui/main/apps/v4/registry/new-york-v4/ui/{component-name}.tsx
```

Extract the exact Tailwind CSS class strings for every sub-element. Pay attention to:
- **Focus ring pattern**: shadcn v4 uses `outline-none focus-visible:border-ring focus-visible:ring-[3px] focus-visible:ring-ring/50`
- **Disabled pattern**: `disabled:pointer-events-none disabled:opacity-50`
- **SVG reset**: `[&_svg]:pointer-events-none [&_svg]:shrink-0 [&_svg:not([class*='size-'])]:size-4`
- **Shadow style**: `shadow-xs` (not `shadow-sm`)
- **Dark mode**: `dark:bg-input/30`, `dark:aria-invalid:ring-destructive/40`
- **Animation**: `data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95`
- **Backdrop opacity**: `bg-black/50` for overlay components

### Step 3: Translate attribute selectors

> Skip this step for styled-only components — they define their own data attributes.

BlazeUI headless components emit different data attributes than Radix (which shadcn targets). Apply these translations when copying classes:

| shadcn (Radix)              | BlazeUI                  |
|-----------------------------|--------------------------|
| `data-[state=open]`        | `data-[open]`            |
| `data-[state=closed]`      | `data-[closed]`          |
| `data-[state=checked]`     | `data-[checked]`         |
| `data-[state=unchecked]`   | (no attribute)           |
| `data-[state=on]`          | `data-[pressed]`         |
| `data-[state=active]`      | `data-[active]`          |
| `data-[state=indeterminate]` | `data-[indeterminate]` |
| `data-[disabled]`          | `data-[disabled]`        |

For overlay components (Dialog, Drawer, AlertDialog, Popover, Menu, etc.), add `data-[closed]:hidden` to backdrop and popup elements. The BlazeUI portal always renders content in the DOM; `hidden` prevents it from showing when closed.

### Step 4: Create the CLI template(s)

Create template(s) at `src/BlazeUI.UI.CLI/Templates/{ComponentName}.razor`.

#### Common template rules (all components)
- Use `@namespace __NAMESPACE__` (the CLI replaces this on install)
- Import `@using BlazeUI.Headless.Core` (for `Css.Cn()`)
- Expose key parameters: `ChildContent`, `Class`, variant/size enums, state bindings
- Add `[Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes` and pass via `@attributes`

#### Standard components (with headless layer)
- Use the headless component's fully qualified names (e.g. `BlazeUI.Headless.Components.Dialog.DialogRoot`)
- `ComponentBase`-derived Roots (AccordionRoot, TabsRoot, etc.) don't accept `Class` or `@attributes` -- wrap in a `<div>` instead

#### Styled-only components (no headless layer)

These components manage their own state and typically decompose into many sub-components:

**State management**: Create a context class in `src/BlazeUI.UI/Components/{ComponentName}Context.cs` to hold shared state. The provider component cascades it via `<CascadingValue>`. Child components consume it via `[CascadingParameter]`. Example:

```csharp
public class SidebarContext
{
    public bool Open { get; set; } = true;
    public EventCallback ToggleSidebar { get; set; }
    public string State => Open ? "expanded" : "collapsed";
}
```

**Multi-file decomposition**: Create one template per sub-component with a shared prefix:
- `{Name}Provider.razor` — state management, cascading value, root wrapper
- `{Name}Header.razor`, `{Name}Content.razor`, `{Name}Footer.razor` — structural sections
- `{Name}Menu.razor`, `{Name}MenuItem.razor`, `{Name}MenuButton.razor` — interactive parts
- `{Name}Trigger.razor` — toggle/control button
- `{Name}Separator.razor` — divider

Simple sub-components (just a div with classes) are typically under 15 lines. Don't overthink these — they're intentionally thin wrappers.

**Custom theme tokens**: If the component has its own color scheme (e.g. sidebar uses `sidebar-background`, `sidebar-accent`), add the CSS custom properties to the demo app's `Styles/input.css`:
1. Add `--blazeui-{component}-*` variables in the `@layer base { :root { ... } }` and `.dark { ... }` blocks
2. Map them in `@theme { --color-{component}-*: var(--blazeui-{component}-*); }`

**Data attributes**: Define your own directly on HTML elements (e.g. `data-state`, `data-collapsible`, `data-variant`). Use them for Tailwind conditional styling with `group-data-[...]` modifiers.

### Step 5: Regenerate compiled components

After editing any template, regenerate the compiled components:

```bash
cd /path/to/blazeui
for f in src/BlazeUI.UI.CLI/Templates/*.razor; do
  sed 's/@namespace __NAMESPACE__/@namespace BlazeUI.UI.Components/' "$f" \
    > "src/BlazeUI.UI/Components/$(basename "$f")"
done
```

Verify the build succeeds:

```bash
dotnet build src/BlazeUI.UI/BlazeUI.UI.csproj
```

### Step 6: Create the demo page

Add a demo page at `test/BlazeUI.Styled.Demo/Components/Pages/{ComponentName}Page.razor`.

- Use `@page "/{kebab-case-name}"`
- Show the component in its key configurations (variants, sizes, disabled state)
- Use the styled component from `BlazeUI.UI.Components` (imported via `_Imports.razor`)
- For headless sub-components used directly in the page, use fully qualified names

Add a navigation link in `MainLayout.razor`'s sidebar navigation arrays (the demo app uses a sidebar layout with component lists grouped by category).

**Layout components** (Sidebar, etc.) may also need integration into `MainLayout.razor` itself, beyond just a demo page. For example, the Sidebar is used as the demo app's actual navigation. In these cases, create both:
1. A demo page explaining the component's sub-components and features
2. The actual integration into the app layout

### Step 7: Rebuild CSS and restart the demo app

```bash
cd test/BlazeUI.Styled.Demo
npx @tailwindcss/cli -i Styles/input.css -o wwwroot/css/app.css --minify
```

If new classes aren't appearing in the output, check:
1. The `@source` directives in `Styles/input.css` cover the file locations
2. `.razor` is being scanned (Tailwind v4 needs explicit `@source` paths for non-default extensions)

Start/restart the demo app:

```bash
lsof -ti:5199 | xargs kill -9 2>/dev/null
sleep 1
ASPNETCORE_ENVIRONMENT=Development dotnet run --project test/BlazeUI.Styled.Demo --urls http://127.0.0.1:5199 &
sleep 6
curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1:5199
```

### Step 8: Visually verify with playwright-cli

Open the component page and take a screenshot:

```bash
playwright-cli open http://127.0.0.1:5199/{kebab-case-name}
playwright-cli screenshot --filename=verify-{component}.png
```

Read the screenshot to verify:
1. **Colors**: themed colors (primary, destructive, muted, accent) render correctly
2. **Variants**: each variant is visually distinct
3. **Sizes**: size differences are apparent
4. **Borders/shadows**: match shadcn appearance
5. **Disabled state**: reduced opacity, no pointer events

For interactive components, test open/close states:

```bash
# Click the trigger
playwright-cli click e{ref}
playwright-cli screenshot --filename=verify-{component}-open.png
```

Compare the open state against shadcn's expected appearance:
- Overlay components: semi-transparent backdrop (`bg-black/50`), centered popup with border and shadow
- Dropdown/popover: positioned relative to trigger, rounded border, shadow
- Accordion/collapsible: content revealed with animation classes

### Step 9: Fix and iterate

If the visual doesn't match:
1. Identify the mismatched classes by inspecting computed styles:
   ```bash
   playwright-cli eval "getComputedStyle(document.querySelector('{selector}')).{property}"
   ```
2. Update the **CLI template** (not the compiled component)
3. Regenerate compiled components (Step 5)
4. Rebuild CSS (Step 7)
5. Restart the demo app and re-screenshot

Repeat until the component matches shadcn's visual appearance.

### Step 10: Clean up

```bash
playwright-cli close
```

Remove any temporary screenshot files. Update `SESSION.md` with any deferred items or known issues.

## Troubleshooting

### Classes not appearing in compiled CSS
The `@source` directives in `test/BlazeUI.Styled.Demo/Styles/input.css` must point to the directories containing `.razor` files. Tailwind v4 doesn't scan `.razor` by default but DOES scan them when explicitly pointed to the directory.

### Overlay backdrop visible when closed
Add `data-[closed]:hidden` to both the backdrop and popup elements. BlazeUI portals always render content in the DOM regardless of open state.

### Demo page shows as dark/broken
The theme CSS variables must be defined in `Styles/input.css` inside a `@layer base {}` block and mapped via `@theme {}`. If the `@import url()` approach is used, CSS ordering rules may drop the import.

### Component throws during SSR
`ComponentBase`-derived Roots don't accept unmatched HTML attributes. Never add `data-testid`, `class`, or `@attributes` to them directly. Wrap in a `<div>` instead.
