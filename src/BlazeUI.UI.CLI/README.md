# BlazeUI.UI.CLI

CLI tool for scaffolding styled [BlazeUI](https://github.com/rabuckley/blazeui) components into your Blazor project — inspired by [shadcn/ui](https://ui.shadcn.com/).

## Installation

```bash
dotnet tool install -g BlazeUI.UI.CLI
```

## Usage

Initialize your project:

```bash
blazeui init
```

Add components:

```bash
blazeui add button
blazeui add dialog
blazeui add --all
```

Components are scaffolded as source files into your project (not referenced as a library), so you own and can customize them.

## License

[MIT](https://github.com/rabuckley/blazeui/blob/main/LICENSE)
