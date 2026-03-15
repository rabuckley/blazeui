using System.CommandLine;
using System.IO.Abstractions;
using System.Reflection;
using System.Text.Json;
using BlazeUI.UI.CLI.Registry;
using Spectre.Console;

namespace BlazeUI.UI.CLI.Commands;

internal class AddCommand
{
    private readonly IFileSystem _fs;
    private readonly IAnsiConsole _console;

    public AddCommand(IFileSystem fs, IAnsiConsole console)
    {
        _fs = fs;
        _console = console;
    }

    public Command Create()
    {
        var componentArgument = new Argument<string?>("component")
        {
            Description = "Component name to add (e.g. button, dialog)",
            Arity = ArgumentArity.ZeroOrOne
        };
        componentArgument.HelpName = "component";

        var overwriteOption = new Option<bool>("--overwrite")
        {
            Description = "Overwrite existing component files"
        };

        var allOption = new Option<bool>("--all")
        {
            Description = "Add all available components"
        };

        var dryRunOption = new Option<bool>("--dry-run")
        {
            Description = "Preview files without writing"
        };

        var pathOption = new Option<DirectoryInfo?>("--path")
        {
            Description = "Project directory (defaults to current directory)"
        };

        var command = new Command("add", "Add a styled component to your project")
        {
            componentArgument,
            overwriteOption,
            allOption,
            dryRunOption,
            pathOption
        };

        command.SetAction(parseResult =>
        {
            var component = parseResult.GetValue(componentArgument);
            var overwrite = parseResult.GetValue(overwriteOption);
            var all = parseResult.GetValue(allOption);
            var dryRun = parseResult.GetValue(dryRunOption);
            var path = parseResult.GetValue(pathOption)?.FullName ?? _fs.Directory.GetCurrentDirectory();

            return Execute(component, overwrite, all, dryRun, path);
        });

        return command;
    }

    internal int Execute(string? componentName, bool overwrite, bool all, bool dryRun, string path)
    {
        var (config, configPath) = ConfigLoader.LoadConfig(_fs, path);
        if (config is null)
        {
            _console.MarkupLine("[red]✗[/] blazeui.json not found. Run 'blazeui init' first.");
            return 1;
        }

        var registry = ComponentRegistry.Load();
        var projectDir = _fs.Path.GetDirectoryName(configPath)!;

        List<ComponentDefinition> toInstall;
        if (all)
        {
            // Resolve all components with their dependencies in correct order.
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            toInstall = [];
            foreach (var comp in registry.All)
            {
                foreach (var resolved in registry.ResolveWithDependencies(comp.Name))
                {
                    if (seen.Add(resolved.Name))
                        toInstall.Add(resolved);
                }
            }
        }
        else
        {
            if (string.IsNullOrEmpty(componentName))
            {
                _console.MarkupLine("[red]✗[/] Specify a component name or use --all.");
                return 1;
            }

            if (registry.Find(componentName) is null)
            {
                _console.MarkupLine($"[red]✗[/] Unknown component: {Markup.Escape(componentName)}");
                _console.MarkupLine("Run 'blazeui list' to see available components.");
                return 1;
            }

            toInstall = registry.ResolveWithDependencies(componentName);
        }

        var assembly = Assembly.GetExecutingAssembly();
        var installed = new List<string>();

        // Ensure shared utilities are scaffolded — every styled component depends
        // on these, similar to shadcn's lib/utils.ts.
        ScaffoldUtility(assembly, "Css.cs", config, projectDir, overwrite, dryRun);
        ScaffoldUtility(assembly, "ButtonClasses.cs", config, projectDir, overwrite, dryRun);

        foreach (var component in toInstall)
        {
            foreach (var file in component.Files)
            {
                // JS files go to wwwroot/js/ rather than the component directory.
                var isJs = file.EndsWith(".js", StringComparison.OrdinalIgnoreCase);
                var outputDir = isJs
                    ? _fs.Path.Combine(projectDir, "wwwroot", "js")
                    : _fs.Path.Combine(projectDir, config.ComponentPath);
                var outputPath = _fs.Path.Combine(outputDir, file);

                if (_fs.File.Exists(outputPath) && !overwrite)
                {
                    _console.MarkupLine($"[yellow]○[/] skip {Markup.Escape(file)} (already exists, use --overwrite to replace)");
                    continue;
                }

                var resourceName = $"BlazeUI.UI.CLI.Templates.{file}";
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is null)
                {
                    _console.MarkupLine($"[red]✗[/] embedded template {Markup.Escape(file)} not found");
                    continue;
                }

                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();
                content = content.Replace("__NAMESPACE__", config.Namespace);

                if (dryRun)
                {
                    _console.WriteLine($"  would write {config.ComponentPath}/{file}");
                }
                else
                {
                    _fs.Directory.CreateDirectory(outputDir);
                    _fs.File.WriteAllText(outputPath, content);
                    _console.MarkupLine($"[green]✓[/] added {Markup.Escape(config.ComponentPath)}/{Markup.Escape(file)}");
                }
            }

            if (!config.Installed.Contains(component.Name, StringComparer.OrdinalIgnoreCase))
                installed.Add(component.Name);
        }

        // Update blazeui.json with newly installed components.
        if (!dryRun && installed.Count > 0)
        {
            config.Installed.AddRange(installed);
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            _fs.File.WriteAllText(configPath, JsonSerializer.Serialize(config, jsonOptions));
        }

        if (!dryRun)
        {
            WarnIfPortalHostMissing(projectDir, toInstall);
        }

        return 0;
    }

    private void WarnIfPortalHostMissing(string projectDir, IReadOnlyList<ComponentDefinition> components)
    {
        if (!components.Any(c => c.Category is "overlay"))
            return;

        var candidates = new[]
        {
            _fs.Path.Combine(projectDir, "Components", "Layout", "MainLayout.razor"),
            _fs.Path.Combine(projectDir, "Shared", "MainLayout.razor"),
            _fs.Path.Combine(projectDir, "Layout", "MainLayout.razor"),
        };

        var layoutPath = candidates.FirstOrDefault(_fs.File.Exists);
        if (layoutPath is null)
            return;

        if (_fs.File.ReadAllText(layoutPath).Contains("<PortalHost"))
            return;

        _console.WriteLine();
        _console.MarkupLine("[yellow]![/] Overlay components require [bold]<PortalHost />[/] in your layout.");
        _console.MarkupLine("  Add to MainLayout.razor:");
        _console.MarkupLine("[dim]    @using BlazeUI.Headless.Overlay[/]");
        _console.MarkupLine("[dim]    <PortalHost />[/]");
    }

    private void ScaffoldUtility(Assembly assembly, string fileName, BlazeUIConfig config, string projectDir, bool overwrite, bool dryRun)
    {
        var outputDir = _fs.Path.Combine(projectDir, config.ComponentPath);
        var outputPath = _fs.Path.Combine(outputDir, fileName);

        if (_fs.File.Exists(outputPath) && !overwrite)
            return;

        var resourceName = $"BlazeUI.UI.CLI.Templates.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return;

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        content = content.Replace("__NAMESPACE__", config.Namespace);

        if (dryRun)
        {
            _console.WriteLine($"  would write {config.ComponentPath}/{fileName}");
        }
        else
        {
            _fs.Directory.CreateDirectory(outputDir);
            _fs.File.WriteAllText(outputPath, content);
            _console.MarkupLine($"[green]✓[/] added {Markup.Escape(config.ComponentPath)}/{Markup.Escape(fileName)}");
        }
    }
}
