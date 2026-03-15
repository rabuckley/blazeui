# BlazeUI.Sonner

Toast notification components for Blazor — inspired by [Sonner](https://sonner.emilkowal.dev/).

## Installation

```bash
dotnet add package BlazeUI.Sonner
```

## Usage

Add the `<Toaster />` component to your layout:

```razor
@using BlazeUI.Sonner

<Toaster />
```

Dispatch toasts from anywhere:

```csharp
@inject IToastService Toasts

Toasts.Success("Changes saved.");
```

## License

[MIT](https://github.com/rabuckley/blazeui/blob/main/LICENSE)
