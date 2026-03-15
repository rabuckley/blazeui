# BlazeUI.Headless

Accessible, unstyled UI components for Blazor — inspired by [Base UI](https://base-ui.com/).

Components ship with full keyboard navigation, ARIA attributes, and work across all Blazor render modes (Server, WebAssembly, Auto).

## Installation

```bash
dotnet add package BlazeUI.Headless
```

Register services in `Program.cs`:

```csharp
builder.Services.AddBlazeUI();
```

## Components

Accordion, AlertDialog, Autocomplete, Avatar, Checkbox, CheckboxGroup, Collapsible, Combobox, ContextMenu, Dialog, Drawer, Field, Fieldset, Form, Input, InputOTP, Menu, Menubar, Meter, NavigationMenu, NumberField, Popover, PreviewCard, Progress, Radio, ScrollArea, Select, Separator, Slider, Switch, Tabs, Toast, Toggle, ToggleGroup, Toolbar, Tooltip.

## Styling

BlazeUI.Headless renders no visual styles. Components expose `data-*` attributes (e.g. `data-open`, `data-disabled`, `data-highlighted`) for styling with CSS or Tailwind.

Use the [BlazeUI CLI](https://www.nuget.org/packages/BlazeUI.UI.CLI) to scaffold pre-built Tailwind-styled templates based on [shadcn/ui](https://ui.shadcn.com/).

## License

[MIT](https://github.com/rabuckley/blazeui/blob/main/LICENSE)
