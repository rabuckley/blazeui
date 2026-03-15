# BlazeUI.Bridge

Browser interop bridge for [BlazeUI](https://github.com/rabuckley/blazeui) Blazor components.

Provides JS interop utilities used internally by BlazeUI.Headless, including a mutation queue for batching DOM operations and an event delegation registry that survives Blazor's SSR-to-interactive handoff.

## Installation

```bash
dotnet add package BlazeUI.Bridge
```

This package is installed automatically as a dependency of `BlazeUI.Headless`. You only need to install it directly if you're building custom components that need its interop primitives.

## License

[MIT](https://github.com/rabuckley/blazeui/blob/main/LICENSE)
