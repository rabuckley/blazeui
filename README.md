# BlazeUI

Accessible, unstyled UI components for Blazor — inspired by [Base UI](https://base-ui.com/) and [shadcn/ui](https://ui.shadcn.com/).

## Packages

| Package | Description |
|---|---|
| **BlazeUI.Headless** | Headless UI components for Blazor, inspired by Base UI |
| **BlazeUI.Bridge** | Browser interop bridge for BlazeUI components |
| **BlazeUI.Sonner** | Toast notification components, inspired by Sonner |
| **BlazeUI.UI.CLI** | CLI tool for scaffolding styled components, inspired by shadcn/ui |

## Getting Started

Install the headless component library:

```bash
dotnet add package BlazeUI.Headless
```

Register services in `Program.cs`:

```csharp
builder.Services.AddBlazeUI();
```

Scaffold styled components with the CLI:

```bash
dotnet tool install -g BlazeUI.UI.CLI
blazeui add button
```

## License

[MIT](LICENSE)
