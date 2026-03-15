using System.CommandLine;
using System.IO.Abstractions;
using System.Reflection;
using BlazeUI.UI.CLI.Registry;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Spectre.Console;

namespace BlazeUI.UI.CLI.Commands;

internal class UpdateCommand
{
    private readonly IFileSystem _fs;
    private readonly IAnsiConsole _console;

    public UpdateCommand(IFileSystem fs, IAnsiConsole console)
    {
        _fs = fs;
        _console = console;
    }

    public Command Create()
    {
        var componentArgument = new Argument<string>("component")
        {
            Description = "Component name to update"
        };

        var diffOption = new Option<bool>("--diff")
        {
            Description = "Show diff between installed and latest template"
        };

        var forceOption = new Option<bool>("--force")
        {
            Description = "Overwrite even if local modifications detected"
        };

        var pathOption = new Option<DirectoryInfo?>("--path")
        {
            Description = "Project directory (defaults to current directory)"
        };

        var command = new Command("update", "Update an installed component to the latest template")
        {
            componentArgument,
            diffOption,
            forceOption,
            pathOption
        };

        command.SetAction(parseResult =>
        {
            var component = parseResult.GetValue(componentArgument);
            var diff = parseResult.GetValue(diffOption);
            var force = parseResult.GetValue(forceOption);
            var path = parseResult.GetValue(pathOption)?.FullName ?? _fs.Directory.GetCurrentDirectory();

            return Execute(component!, diff, force, path);
        });

        return command;
    }

    internal int Execute(string componentName, bool diff, bool force, string path)
    {
        var (config, configPath) = ConfigLoader.LoadConfig(_fs, path);
        if (config is null)
        {
            _console.MarkupLine("[red]✗[/] blazeui.json not found. Run 'blazeui init' first.");
            return 1;
        }

        var registry = ComponentRegistry.Load();
        var component = registry.Find(componentName);
        if (component is null)
        {
            _console.MarkupLine($"[red]✗[/] Unknown component: {Markup.Escape(componentName)}");
            return 1;
        }

        if (!config.Installed.Contains(componentName, StringComparer.OrdinalIgnoreCase))
        {
            _console.MarkupLine($"[red]✗[/] Component '{Markup.Escape(componentName)}' is not installed. Use 'blazeui add {Markup.Escape(componentName)}' instead.");
            return 1;
        }

        var projectDir = _fs.Path.GetDirectoryName(configPath)!;
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var file in component.Files)
        {
            var installedPath = _fs.Path.Combine(projectDir, config.ComponentPath, file);
            var resourceName = $"BlazeUI.UI.CLI.Templates.{file}";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                _console.MarkupLine($"[red]✗[/] embedded template {Markup.Escape(file)} not found");
                continue;
            }

            using var reader = new StreamReader(stream);
            var templateContent = reader.ReadToEnd()
                .Replace("__NAMESPACE__", config.Namespace);

            if (!_fs.File.Exists(installedPath))
            {
                _console.MarkupLine($"[green]✓[/] {Markup.Escape(file)} not found on disk — writing fresh copy");
                var dir = _fs.Path.GetDirectoryName(installedPath)!;
                _fs.Directory.CreateDirectory(dir);
                _fs.File.WriteAllText(installedPath, templateContent);
                continue;
            }

            var installedContent = _fs.File.ReadAllText(installedPath);
            if (installedContent == templateContent)
            {
                _console.MarkupLine($"[dim]·[/] {Markup.Escape(file)} is up to date");
                continue;
            }

            if (diff)
            {
                _console.MarkupLine($"  {Markup.Escape(file)} differs from latest template:");
                ShowUnifiedDiff(file, installedContent, templateContent);
                continue;
            }

            if (!force)
            {
                _console.MarkupLine($"[yellow]![/] {Markup.Escape(file)} has local modifications. Use --force to overwrite or --diff to preview changes.");
                continue;
            }

            _fs.File.WriteAllText(installedPath, templateContent);
            _console.MarkupLine($"[green]✓[/] updated {Markup.Escape(file)}");
        }

        return 0;
    }

    private void ShowUnifiedDiff(string fileName, string current, string latest)
    {
        var diff = InlineDiffBuilder.Diff(current, latest);
        var lines = diff.Lines.ToList();

        _console.MarkupLine($"[bold]--- installed/{Markup.Escape(fileName)}[/]");
        _console.MarkupLine($"[bold]+++ template/{Markup.Escape(fileName)}[/]");

        // Show only lines within 3 lines of a change (context window).
        var changed = lines.Select((l, i) => (l, i))
            .Where(x => x.l.Type is not ChangeType.Unchanged)
            .Select(x => x.i)
            .ToHashSet();

        var lastPrinted = -1;
        for (var i = 0; i < lines.Count; i++)
        {
            var inContext = changed.Any(c => Math.Abs(c - i) <= 3);
            if (!inContext) continue;

            if (lastPrinted >= 0 && i > lastPrinted + 1)
                _console.MarkupLine("[dim]...[/]");

            var line = lines[i];
            var text = Markup.Escape(line.Text);
            var output = line.Type switch
            {
                ChangeType.Inserted => $"[green]+{text}[/]",
                ChangeType.Deleted  => $"[red]-{text}[/]",
                _ => $" {text}"
            };
            _console.MarkupLine(output);
            lastPrinted = i;
        }
    }
}
