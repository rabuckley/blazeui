using System.CommandLine;
using System.IO.Abstractions;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using BlazeUI.UI.CLI.Themes;
using Spectre.Console;

namespace BlazeUI.UI.CLI.Commands;

internal class InitCommand
{
    private readonly IFileSystem _fs;
    private readonly IAnsiConsole _console;
    private readonly IProcessRunner _runner;

    public InitCommand(IFileSystem fs, IAnsiConsole console, IProcessRunner runner)
    {
        _fs = fs;
        _console = console;
        _runner = runner;
    }

    public Command Create()
    {
        var pathOption = new Option<DirectoryInfo?>("--path")
        {
            Description = "Project directory (defaults to current directory)"
        };

        var componentPathOption = new Option<string?>("--component-path")
        {
            Description = "Component output path (default: Components/UI)"
        };

        var cssPathOption = new Option<string?>("--css-path")
        {
            Description = "CSS output path (default: wwwroot/css)"
        };

        var namespaceOption = new Option<string?>("--namespace")
        {
            Description = "Component namespace"
        };

        var baseColorOption = new Option<string?>("--base-color")
        {
            Description = "Base color theme (neutral, stone, zinc, mauve, olive, mist, taupe)"
        };

        var accentColorOption = new Option<string?>("--accent-color")
        {
            Description = "Accent color for primary/charts (amber, blue, cyan, emerald, fuchsia, green, indigo, lime, orange, pink, purple, red, rose, sky, teal, violet, yellow)"
        };

        var yesOption = new Option<bool>("--yes", "-y")
        {
            Description = "Skip prompts, use defaults"
        };

        var command = new Command("init", "Initialize BlazeUI in your project")
        {
            pathOption,
            componentPathOption,
            cssPathOption,
            namespaceOption,
            baseColorOption,
            accentColorOption,
            yesOption
        };

        command.SetAction(parseResult =>
        {
            var path = parseResult.GetValue(pathOption)?.FullName ?? _fs.Directory.GetCurrentDirectory();
            var componentPath = parseResult.GetValue(componentPathOption);
            var cssPath = parseResult.GetValue(cssPathOption);
            var ns = parseResult.GetValue(namespaceOption);
            var baseColor = parseResult.GetValue(baseColorOption);
            var accentColor = parseResult.GetValue(accentColorOption);
            var yes = parseResult.GetValue(yesOption);
            return Execute(path, componentPath, cssPath, ns, baseColor, accentColor, yes);
        });

        return command;
    }

    internal int Execute(string path, string? componentPath = null, string? cssPath = null,
                         string? ns = null, string? baseColor = null, string? accentColor = null,
                         bool yes = false)
    {
        var csproj = FindCsproj(path);
        if (csproj is null)
        {
            _console.MarkupLine("[red]✗[/] No .csproj file found. Run this command from a Blazor project directory.");
            return 1;
        }

        var projectDir = _fs.Path.GetDirectoryName(csproj)!;
        var configPath = _fs.Path.Combine(projectDir, "blazeui.json");

        if (_fs.File.Exists(configPath))
        {
            // If changing theme on an already-initialized project, regenerate CSS.
            if (baseColor is not null || accentColor is not null)
                return ReapplyTheme(configPath, projectDir, baseColor, accentColor);

            _console.MarkupLine("[red]✗[/] blazeui.json already exists. Project is already initialized.");
            _console.MarkupLine("    To change the theme: blazeui init --base-color <color> --accent-color <color>");
            return 1;
        }

        // Validate --base-color if provided explicitly.
        if (baseColor is not null && BaseColorScale.Find(baseColor) is null)
        {
            var valid = string.Join(", ", BaseColorScale.All.Select(s => s.Name));
            _console.MarkupLine($"[red]✗[/] Unknown base color: {Markup.Escape(baseColor)}. Valid options: {valid}");
            return 1;
        }

        // Validate --accent-color if provided explicitly.
        if (accentColor is not null && AccentColor.Find(accentColor) is null)
        {
            var valid = string.Join(", ", AccentColor.All.Select(a => a.Name));
            _console.MarkupLine($"[red]✗[/] Unknown accent color: {Markup.Escape(accentColor)}. Valid options: {valid}");
            return 1;
        }

        var rootNamespace = DetectRootNamespace(csproj);

        // Resolve configuration values — prompt interactively when possible, otherwise use defaults.
        if (!yes && _console.Profile.Capabilities.Interactive)
        {
            componentPath ??= _console.Prompt(
                new TextPrompt<string>("Component path:").DefaultValue("Components/UI"));
            cssPath ??= _console.Prompt(
                new TextPrompt<string>("CSS path:").DefaultValue("wwwroot/css"));
            ns ??= _console.Prompt(
                new TextPrompt<string>("Namespace:").DefaultValue($"{rootNamespace}.Components.UI"));
            baseColor ??= _console.Prompt(
                new SelectionPrompt<string>()
                    .Title("Base color:")
                    .AddChoices(BaseColorScale.All.Select(s => s.Name)));

            // Accent color is optional — "none" means use the base color's defaults.
            if (accentColor is null)
            {
                var accentChoices = new[] { "none" }.Concat(AccentColor.All.Select(a => a.Name));
                var choice = _console.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Accent color:")
                        .AddChoices(accentChoices));
                accentColor = choice is "none" ? null : choice;
            }
        }
        else
        {
            componentPath ??= "Components/UI";
            cssPath ??= "wwwroot/css";
            ns ??= $"{rootNamespace}.Components.UI";
            baseColor ??= "neutral";
        }

        var config = new BlazeUIConfig
        {
            ComponentPath = componentPath,
            CssPath = cssPath,
            Namespace = ns,
            BaseColor = baseColor,
            AccentColor = accentColor ?? "",
            Installed = []
        };

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        _fs.File.WriteAllText(configPath, JsonSerializer.Serialize(config, jsonOptions));

        // Extract CSS files to the project.
        var cssDir = _fs.Path.Combine(projectDir, config.CssPath);
        _fs.Directory.CreateDirectory(cssDir);

        var assembly = Assembly.GetExecutingAssembly();

        // Write blazeui.css: the structural CSS (animations, variants, @theme) from the embedded
        // resource, with theme variables (`:root` and `.dark`) generated for the chosen base color.
        var blazeuiCssPath = _fs.Path.Combine(cssDir, "blazeui.css");
        ExtractResource(assembly, "BlazeUI.UI.CLI.Styles.blazeui.css", blazeuiCssPath);

        var scale = BaseColorScale.Find(baseColor)!;
        var accent = !string.IsNullOrEmpty(accentColor) ? AccentColor.Find(accentColor) : null;
        _fs.File.AppendAllText(blazeuiCssPath, scale.GenerateThemeCss(accent));

        // Extract the Tailwind entry point into Styles/ (sibling to wwwroot/).
        var stylesDir = _fs.Path.Combine(projectDir, "Styles");
        _fs.Directory.CreateDirectory(stylesDir);
        var inputCssPath = _fs.Path.Combine(stylesDir, "input.css");

        ExtractResource(assembly, "BlazeUI.UI.CLI.Styles.input.css", inputCssPath);

        // Scaffold package.json with required npm dependencies if it doesn't exist.
        var packageJsonPath = _fs.Path.Combine(projectDir, "package.json");
        var createdPackageJson = false;
        if (!_fs.File.Exists(packageJsonPath))
        {
            _fs.File.WriteAllText(packageJsonPath, """
                {
                  "private": true,
                  "devDependencies": {
                    "@tailwindcss/cli": "^4",
                    "tw-animate-css": "^1",
                    "@fontsource-variable/geist": "^5"
                  }
                }
                """);
            _console.MarkupLine("[green]✓[/] Created package.json");
            createdPackageJson = true;
        }

        _console.MarkupLine($"[green]✓[/] Created blazeui.json");
        _console.MarkupLine($"[green]✓[/] Created {Markup.Escape(config.CssPath)}/blazeui.css");
        _console.MarkupLine("[green]✓[/] Created Styles/input.css");

        // Auto-install NuGet packages.
        var nugetOk = TryInstallNuGet(csproj, projectDir);

        // Auto-inject services into Program.cs.
        var servicesOk = TryInjectServices(projectDir);

        // Auto-install npm dependencies when we created package.json.
        var npmOk = false;
        if (createdPackageJson)
        {
            var pm = DetectPackageManager(projectDir);
            _console.Status().Start($"Installing npm dependencies ({pm})...", _ =>
            {
                npmOk = _runner.Run(pm, "install", projectDir) == 0;
            });

            if (npmOk)
                _console.MarkupLine($"[green]✓[/] Installed npm dependencies ({pm})");
            else
                _console.MarkupLine($"[yellow]![/] npm install failed — run manually");
        }

        // Print remaining manual steps.
        _console.WriteLine();
        _console.MarkupLine("[bold]Next steps:[/]");

        var step = 1;
        if (!createdPackageJson && !npmOk)
        {
            _console.MarkupLine($"  {step++}. Install npm dependencies: pnpm install");
        }
        if (!nugetOk)
        {
            _console.MarkupLine($"  {step++}. Add NuGet dependencies: dotnet add package TailwindMerge.NET && dotnet add package BlazeUI.Headless");
        }
        if (!servicesOk)
        {
            _console.MarkupLine($"  {step++}. Add services.AddBlazeUI() to your Program.cs");
        }
        _console.MarkupLine($"  {step++}. Build Tailwind: npx @tailwindcss/cli -i Styles/input.css -o wwwroot/css/app.css --minify");
        _console.MarkupLine($"  {step++}. Add to your layout: <link href=\"css/app.css\" rel=\"stylesheet\" />");
        _console.MarkupLine($"  {step++}. Run: dotnet blazeui add button");

        return 0;
    }

    /// <summary>
    /// Regenerates blazeui.css with a new base/accent color on an already-initialized project.
    /// </summary>
    private int ReapplyTheme(string configPath, string projectDir, string? baseColor, string? accentColor)
    {
        var jsonText = _fs.File.ReadAllText(configPath);
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var config = JsonSerializer.Deserialize<BlazeUIConfig>(jsonText, jsonOptions) ?? new BlazeUIConfig();

        // Use provided values or fall back to existing config.
        baseColor ??= config.BaseColor;
        accentColor ??= config.AccentColor;

        var scale = BaseColorScale.Find(baseColor);
        if (scale is null)
        {
            var valid = string.Join(", ", BaseColorScale.All.Select(s => s.Name));
            _console.MarkupLine($"[red]✗[/] Unknown base color: {Markup.Escape(baseColor)}. Valid options: {valid}");
            return 1;
        }

        AccentColor? accent = null;
        if (!string.IsNullOrEmpty(accentColor))
        {
            accent = AccentColor.Find(accentColor);
            if (accent is null)
            {
                var valid = string.Join(", ", AccentColor.All.Select(a => a.Name));
                _console.MarkupLine($"[red]✗[/] Unknown accent color: {Markup.Escape(accentColor)}. Valid options: {valid}");
                return 1;
            }
        }

        // Update config.
        config.BaseColor = baseColor;
        config.AccentColor = accentColor ?? "";
        _fs.File.WriteAllText(configPath, JsonSerializer.Serialize(config, jsonOptions));

        // Regenerate blazeui.css: structural CSS + chosen theme.
        var cssDir = _fs.Path.Combine(projectDir, config.CssPath);
        _fs.Directory.CreateDirectory(cssDir);
        var blazeuiCssPath = _fs.Path.Combine(cssDir, "blazeui.css");

        var assembly = Assembly.GetExecutingAssembly();
        ExtractResource(assembly, "BlazeUI.UI.CLI.Styles.blazeui.css", blazeuiCssPath);
        _fs.File.AppendAllText(blazeuiCssPath, scale.GenerateThemeCss(accent));

        var themeDesc = accent is not null ? $"{baseColor} + {accentColor}" : baseColor;
        _console.MarkupLine($"[green]✓[/] Updated theme to [bold]{Markup.Escape(themeDesc)}[/]");
        _console.MarkupLine($"[green]✓[/] Regenerated {Markup.Escape(config.CssPath)}/blazeui.css");
        return 0;
    }

    private bool TryInstallNuGet(string csprojPath, string projectDir)
    {
        var ok = true;
        _console.Status().Start("Installing NuGet packages...", _ =>
        {
            if (_runner.Run("dotnet", $"add \"{csprojPath}\" package TailwindMerge.NET", projectDir) != 0)
                ok = false;
            if (_runner.Run("dotnet", $"add \"{csprojPath}\" package BlazeUI.Headless", projectDir) != 0)
                ok = false;
        });

        if (ok)
            _console.MarkupLine("[green]✓[/] Installed NuGet packages");
        else
            _console.MarkupLine("[yellow]![/] NuGet install failed — run manually");

        return ok;
    }

    private bool TryInjectServices(string projectDir)
    {
        var programPath = _fs.Path.Combine(projectDir, "Program.cs");
        if (!_fs.File.Exists(programPath))
            return false;

        var content = _fs.File.ReadAllText(programPath);
        if (content.Contains("AddBlazeUI"))
            return true;

        // Find the insertion point: the line containing `builder.Build()`.
        var lines = content.Split('\n').ToList();
        var insertIndex = lines.FindIndex(l => l.Contains("builder.Build()"));
        if (insertIndex < 0)
            return false;

        lines.Insert(insertIndex, "builder.Services.AddBlazeUI();");

        // Ensure the using directive is present.
        var joined = string.Join('\n', lines);
        if (!joined.Contains("using BlazeUI.Headless.Core"))
        {
            joined = "using BlazeUI.Headless.Core;\n" + joined;
        }

        _fs.File.WriteAllText(programPath, joined);
        _console.MarkupLine("[green]✓[/] Added AddBlazeUI() to Program.cs");
        return true;
    }

    private string DetectPackageManager(string dir)
    {
        if (_fs.File.Exists(_fs.Path.Combine(dir, "pnpm-lock.yaml")))
            return "pnpm";
        if (_fs.File.Exists(_fs.Path.Combine(dir, "yarn.lock")))
            return "yarn";
        if (_fs.File.Exists(_fs.Path.Combine(dir, "bun.lockb")))
            return "bun";
        return "npm";
    }

    private void ExtractResource(Assembly assembly, string resourceName, string targetPath)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is not null)
        {
            using var writer = _fs.File.Create(targetPath);
            stream.CopyTo(writer);
        }
    }

    private string? FindCsproj(string startDir)
    {
        var dir = startDir;
        while (dir is not null)
        {
            var files = _fs.Directory.GetFiles(dir, "*.csproj");
            if (files.Length > 0)
                return files[0];
            dir = _fs.Directory.GetParent(dir)?.FullName;
        }
        return null;
    }

    private string DetectRootNamespace(string csprojPath)
    {
        try
        {
            var xml = _fs.File.ReadAllText(csprojPath);
            var doc = XDocument.Parse(xml);
            var ns = doc.Descendants("RootNamespace").FirstOrDefault()?.Value;
            if (!string.IsNullOrEmpty(ns))
                return ns;
        }
        catch
        {
            // Fall through to default.
        }

        return _fs.Path.GetFileNameWithoutExtension(csprojPath);
    }
}

internal sealed class BlazeUIConfig
{
    public string ComponentPath { get; set; } = "Components/UI";
    public string CssPath { get; set; } = "wwwroot/css";
    public string Namespace { get; set; } = "";
    public string BaseColor { get; set; } = "neutral";
    public string AccentColor { get; set; } = "";
    public List<string> Installed { get; set; } = [];
}
