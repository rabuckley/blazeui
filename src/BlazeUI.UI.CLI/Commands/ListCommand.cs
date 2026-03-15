using System.CommandLine;
using System.IO.Abstractions;
using BlazeUI.UI.CLI.Registry;
using Spectre.Console;

namespace BlazeUI.UI.CLI.Commands;

internal class ListCommand
{
    private readonly IFileSystem _fs;
    private readonly IAnsiConsole _console;

    public ListCommand(IFileSystem fs, IAnsiConsole console)
    {
        _fs = fs;
        _console = console;
    }

    public Command Create()
    {
        var pathOption = new Option<DirectoryInfo?>("--path")
        {
            Description = "Project directory (defaults to current directory)"
        };

        var command = new Command("list", "List available components")
        {
            pathOption
        };

        command.SetAction(parseResult =>
        {
            var path = parseResult.GetValue(pathOption)?.FullName ?? _fs.Directory.GetCurrentDirectory();
            return Execute(path);
        });

        return command;
    }

    internal int Execute(string path)
    {
        var registry = ComponentRegistry.Load();
        var (config, _) = ConfigLoader.LoadConfig(_fs, path);
        var installed = config?.Installed ?? [];

        _console.MarkupLine("[bold]Available components:[/]");
        _console.WriteLine();

        var grouped = registry.All
            .GroupBy(c => c.Category)
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            _console.MarkupLine($"  [bold]{Markup.Escape(group.Key)}:[/]");
            foreach (var component in group)
            {
                var isInstalled = installed.Contains(component.Name, StringComparer.OrdinalIgnoreCase);
                var status = isInstalled ? "[green]✓[/]" : " ";
                var deps = component.Deps.Count > 0
                    ? $" [dim](deps: {Markup.Escape(string.Join(", ", component.Deps))})[/]"
                    : "";
                _console.MarkupLine($"    {status} {Markup.Escape(component.Name),-16}{deps}");
            }
            _console.WriteLine();
        }

        return 0;
    }
}
