using System.CommandLine;
using System.IO.Abstractions;
using BlazeUI.UI.CLI.Commands;
using Spectre.Console;

var fs = new FileSystem();
var console = AnsiConsole.Console;
var runner = new ProcessRunner();

var rootCommand = new RootCommand("BlazeUI CLI — scaffold styled Blazor components");

rootCommand.Subcommands.Add(new InitCommand(fs, console, runner).Create());
rootCommand.Subcommands.Add(new AddCommand(fs, console).Create());
rootCommand.Subcommands.Add(new ListCommand(fs, console).Create());
rootCommand.Subcommands.Add(new UpdateCommand(fs, console).Create());

return rootCommand.Parse(args).Invoke();
