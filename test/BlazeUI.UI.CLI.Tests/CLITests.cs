using System.IO.Abstractions.TestingHelpers;
using BlazeUI.UI.CLI.Commands;
using BlazeUI.UI.CLI.Registry;
using Spectre.Console.Testing;

namespace BlazeUI.UI.CLI.Tests;

/// <summary>
/// Stub that records calls but never actually runs a process.
/// Returns the configured exit code (default 0).
/// </summary>
internal sealed class FakeProcessRunner : IProcessRunner
{
    public int ExitCode { get; set; }
    public List<(string Command, string Arguments, string WorkingDirectory)> Invocations { get; } = [];

    public int Run(string command, string arguments, string workingDirectory)
    {
        Invocations.Add((command, arguments, workingDirectory));
        return ExitCode;
    }
}

public class InitCommandTests
{
    private static MockFileSystem CreateFileSystem() => new();

    private static void CreateCsproj(MockFileSystem fs, string dir, string name = "TestApp")
    {
        fs.Directory.CreateDirectory(dir);
        fs.File.WriteAllText(fs.Path.Combine(dir, $"{name}.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk.Razor">
              <PropertyGroup>
                <RootNamespace>MyApp.Web</RootNamespace>
              </PropertyGroup>
            </Project>
            """);
    }

    private static (InitCommand Cmd, TestConsole Console, FakeProcessRunner Runner) CreateCommand(MockFileSystem fs)
    {
        var console = new TestConsole();
        var runner = new FakeProcessRunner();
        return (new InitCommand(fs, console, runner), console, runner);
    }

    [Fact]
    public void Init_creates_config_and_css()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        var (cmd, _, _) = CreateCommand(fs);

        // Act
        var exitCode = cmd.Execute("/project");

        // Assert
        Assert.Equal(0, exitCode);
        Assert.True(fs.File.Exists("/project/blazeui.json"));
        Assert.True(fs.File.Exists("/project/wwwroot/css/blazeui.css"));
    }

    [Fact]
    public void Init_detects_root_namespace_from_csproj()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        var (cmd, _, _) = CreateCommand(fs);

        // Act
        cmd.Execute("/project");

        // Assert
        var json = fs.File.ReadAllText("/project/blazeui.json");
        Assert.Contains("MyApp.Web.Components.UI", json);
    }

    [Fact]
    public void Init_fails_when_already_initialized()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        var (cmd, _, _) = CreateCommand(fs);
        cmd.Execute("/project");

        // Act
        var (cmd2, _, _) = CreateCommand(fs);
        var exitCode = cmd2.Execute("/project");

        // Assert
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public void Init_fails_without_csproj()
    {
        // Arrange
        var fs = CreateFileSystem();
        fs.Directory.CreateDirectory("/project");
        var (cmd, _, _) = CreateCommand(fs);

        // Act
        var exitCode = cmd.Execute("/project");

        // Assert
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public void Init_uses_custom_paths_when_provided()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        var (cmd, _, _) = CreateCommand(fs);

        // Act
        var exitCode = cmd.Execute("/project",
            componentPath: "UI/Components",
            cssPath: "wwwroot/styles",
            ns: "Custom.Namespace");

        // Assert
        Assert.Equal(0, exitCode);
        var json = fs.File.ReadAllText("/project/blazeui.json");
        Assert.Contains("UI/Components", json);
        Assert.Contains("wwwroot/styles", json);
        Assert.Contains("Custom.Namespace", json);
    }

    [Fact]
    public void Init_injects_services_into_program_cs()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        fs.File.WriteAllText("/project/Program.cs", """
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();
            app.Run();
            """);
        var (cmd, console, _) = CreateCommand(fs);

        // Act
        cmd.Execute("/project");

        // Assert
        var program = fs.File.ReadAllText("/project/Program.cs");
        Assert.Contains("AddBlazeUI()", program);
        Assert.Contains("using BlazeUI.Headless.Core", program);
        Assert.Contains("Added AddBlazeUI()", console.Output);
    }

    [Fact]
    public void Init_injects_using_after_header_comments()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        fs.File.WriteAllText("/project/Program.cs",
            "// Licensed to the .NET Foundation under one or more agreements.\n" +
            "// The .NET Foundation licenses this file to you under the MIT license.\n" +
            "\n" +
            "var builder = WebApplication.CreateBuilder(args);\n" +
            "var app = builder.Build();\n" +
            "app.Run();\n");
        var (cmd, _, _) = CreateCommand(fs);

        // Act
        cmd.Execute("/project");

        // Assert — using directive appears after the header comments, not before them.
        var program = fs.File.ReadAllText("/project/Program.cs");
        var usingIndex = program.IndexOf("using BlazeUI.Headless.Core");
        var headerEnd = program.IndexOf("MIT license.");
        Assert.True(usingIndex > headerEnd,
            "using directive should appear after header comments");
    }

    [Fact]
    public void Init_skips_service_injection_when_already_present()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        fs.File.WriteAllText("/project/Program.cs", """
            using BlazeUI.Headless.Core;
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddBlazeUI();
            var app = builder.Build();
            app.Run();
            """);
        var (cmd, _, _) = CreateCommand(fs);

        // Act
        cmd.Execute("/project");

        // Assert — Program.cs unchanged (AddBlazeUI appears exactly once)
        var program = fs.File.ReadAllText("/project/Program.cs");
        var count = program.Split("AddBlazeUI").Length - 1;
        Assert.Equal(1, count);
    }

    [Fact]
    public void Init_injects_using_into_imports_razor()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        fs.File.WriteAllText("/project/Program.cs",
            "var builder = WebApplication.CreateBuilder(args);\nvar app = builder.Build();\napp.Run();\n");
        fs.File.WriteAllText("/project/_Imports.razor",
            "@using Microsoft.AspNetCore.Components.Web\n");
        var (cmd, console, _) = CreateCommand(fs);

        // Act
        cmd.Execute("/project");

        // Assert
        var imports = fs.File.ReadAllText("/project/_Imports.razor");
        Assert.Contains("@using BlazeUI.Headless.Core", imports);
        Assert.Contains("Added @using BlazeUI.Headless.Core", console.Output);
    }

    [Fact]
    public void Init_skips_imports_injection_when_already_present()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        fs.File.WriteAllText("/project/Program.cs",
            "var builder = WebApplication.CreateBuilder(args);\nvar app = builder.Build();\napp.Run();\n");
        fs.File.WriteAllText("/project/_Imports.razor",
            "@using BlazeUI.Headless.Core\n");
        var (cmd, console, _) = CreateCommand(fs);

        // Act
        cmd.Execute("/project");

        // Assert — no duplicate directive added.
        var imports = fs.File.ReadAllText("/project/_Imports.razor");
        var count = imports.Split("@using BlazeUI.Headless.Core").Length - 1;
        Assert.Equal(1, count);
        Assert.DoesNotContain("Added @using BlazeUI.Headless.Core", console.Output);
    }

    [Fact]
    public void Init_finds_imports_in_components_subfolder()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        fs.File.WriteAllText("/project/Program.cs",
            "var builder = WebApplication.CreateBuilder(args);\nvar app = builder.Build();\napp.Run();\n");
        fs.Directory.CreateDirectory("/project/Components");
        fs.File.WriteAllText("/project/Components/_Imports.razor",
            "@using Microsoft.AspNetCore.Components.Routing\n");
        var (cmd, console, _) = CreateCommand(fs);

        // Act
        cmd.Execute("/project");

        // Assert
        var imports = fs.File.ReadAllText("/project/Components/_Imports.razor");
        Assert.Contains("@using BlazeUI.Headless.Core", imports);
    }

    [Fact]
    public void Init_runs_nuget_install()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        var (cmd, console, runner) = CreateCommand(fs);

        // Act
        cmd.Execute("/project");

        // Assert
        Assert.Contains(runner.Invocations, i => i.Command == "dotnet" && i.Arguments.Contains("TailwindMerge.NET"));
        Assert.Contains(runner.Invocations, i => i.Command == "dotnet" && i.Arguments.Contains("BlazeUI.Headless"));
        Assert.Contains("Installed NuGet packages", console.Output);
    }

    [Fact]
    public void Init_runs_npm_install_when_package_json_created()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        var (cmd, _, runner) = CreateCommand(fs);

        // Act
        cmd.Execute("/project");

        // Assert — npm install was called since package.json was created
        Assert.Contains(runner.Invocations, i => i.Command == "npm" && i.Arguments == "install");
    }

    [Fact]
    public void Init_detects_pnpm_lock()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        fs.File.WriteAllText("/project/pnpm-lock.yaml", "");
        var (cmd, _, runner) = CreateCommand(fs);

        // Act
        cmd.Execute("/project");

        // Assert
        Assert.Contains(runner.Invocations, i => i.Command == "pnpm" && i.Arguments == "install");
    }

    [Fact]
    public void Init_skips_npm_install_when_package_json_preexists()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        fs.File.WriteAllText("/project/package.json", "{}");
        var (cmd, _, runner) = CreateCommand(fs);

        // Act
        cmd.Execute("/project");

        // Assert — no npm/pnpm install since package.json already existed
        Assert.DoesNotContain(runner.Invocations, i => i.Arguments == "install");
    }

    [Fact]
    public void Init_defaults_to_neutral_base_color()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        var (cmd, _, _) = CreateCommand(fs);

        // Act
        cmd.Execute("/project");

        // Assert
        var json = fs.File.ReadAllText("/project/blazeui.json");
        Assert.Contains("\"baseColor\": \"neutral\"", json);

        var css = fs.File.ReadAllText("/project/wwwroot/css/blazeui.css");
        Assert.Contains("--primary: oklch(0.205 0 0)", css);
    }

    [Fact]
    public void Init_applies_chosen_base_color()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        var (cmd, _, _) = CreateCommand(fs);

        // Act
        cmd.Execute("/project", baseColor: "zinc");

        // Assert — zinc uses a different hue in its gray scale
        var json = fs.File.ReadAllText("/project/blazeui.json");
        Assert.Contains("\"baseColor\": \"zinc\"", json);

        var css = fs.File.ReadAllText("/project/wwwroot/css/blazeui.css");
        Assert.Contains("--primary: oklch(0.21 0.006 285.885)", css);
        Assert.Contains("--foreground: oklch(0.141 0.005 285.823)", css);
    }

    [Fact]
    public void Init_rejects_unknown_base_color()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        var (cmd, console, _) = CreateCommand(fs);

        // Act
        var exitCode = cmd.Execute("/project", baseColor: "purple");

        // Assert
        Assert.Equal(1, exitCode);
        Assert.Contains("Unknown base color", console.Output);
    }

    [Fact]
    public void Init_reapplies_theme_on_already_initialized_project()
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        var (cmd, _, _) = CreateCommand(fs);
        cmd.Execute("/project"); // initial setup with neutral

        // Act — reapply with zinc on already-initialized project
        var (cmd2, console2, _) = CreateCommand(fs);
        var exitCode = cmd2.Execute("/project", baseColor: "zinc");

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("Updated theme", console2.Output);

        var json = fs.File.ReadAllText("/project/blazeui.json");
        Assert.Contains("\"baseColor\": \"zinc\"", json);

        var css = fs.File.ReadAllText("/project/wwwroot/css/blazeui.css");
        Assert.Contains("oklch(0.21 0.006 285.885)", css);
    }

    [Theory]
    [InlineData("neutral")]
    [InlineData("stone")]
    [InlineData("zinc")]
    [InlineData("mauve")]
    [InlineData("olive")]
    [InlineData("mist")]
    [InlineData("taupe")]
    public void Init_generates_valid_css_for_all_base_colors(string baseColor)
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        var (cmd, _, _) = CreateCommand(fs);

        // Act
        var exitCode = cmd.Execute("/project", baseColor: baseColor);

        // Assert — CSS contains both light and dark theme blocks
        Assert.Equal(0, exitCode);
        var css = fs.File.ReadAllText("/project/wwwroot/css/blazeui.css");
        Assert.Contains(":root {", css);
        Assert.Contains(".dark {", css);
        Assert.Contains("--primary:", css);
        Assert.Contains("--background:", css);
    }

    [Theory]
    [InlineData("amber")]
    [InlineData("blue")]
    [InlineData("cyan")]
    [InlineData("emerald")]
    [InlineData("fuchsia")]
    [InlineData("green")]
    [InlineData("indigo")]
    [InlineData("lime")]
    [InlineData("orange")]
    [InlineData("pink")]
    [InlineData("purple")]
    [InlineData("red")]
    [InlineData("rose")]
    [InlineData("sky")]
    [InlineData("teal")]
    [InlineData("violet")]
    [InlineData("yellow")]
    public void Init_generates_valid_css_for_all_accent_colors(string accentColor)
    {
        // Arrange
        var fs = CreateFileSystem();
        CreateCsproj(fs, "/project");
        var (cmd, _, _) = CreateCommand(fs);

        // Act
        var exitCode = cmd.Execute("/project", accentColor: accentColor);

        // Assert — CSS contains both light and dark theme blocks with accent-specific primary values
        Assert.Equal(0, exitCode);
        var css = fs.File.ReadAllText("/project/wwwroot/css/blazeui.css");
        Assert.Contains(":root {", css);
        Assert.Contains(".dark {", css);
        Assert.Contains("--primary:", css);
    }
}

public class AddCommandTests
{
    private static (MockFileSystem Fs, string Dir, TestConsole Console) SetupProject()
    {
        var fs = new MockFileSystem();
        var console = new TestConsole();
        var runner = new FakeProcessRunner();
        var dir = "/project";
        fs.Directory.CreateDirectory(dir);
        fs.File.WriteAllText(fs.Path.Combine(dir, "TestApp.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk.Razor">
              <PropertyGroup>
                <RootNamespace>TestApp</RootNamespace>
              </PropertyGroup>
            </Project>
            """);
        new InitCommand(fs, console, runner).Execute(dir);
        return (fs, dir, console);
    }

    [Fact]
    public void Add_scaffolds_file_with_correct_namespace()
    {
        // Arrange
        var (fs, dir, _) = SetupProject();
        var console = new TestConsole();

        // Act
        var exitCode = new AddCommand(fs, console).Execute("button", overwrite: false, all: false, dryRun: false, dir);

        // Assert
        Assert.Equal(0, exitCode);
        var filePath = fs.Path.Combine(dir, "Components", "UI", "Button.razor");
        Assert.True(fs.File.Exists(filePath));

        var content = fs.File.ReadAllText(filePath);
        Assert.Contains("@namespace TestApp.Components.UI", content);
        Assert.DoesNotContain("__NAMESPACE__", content);
    }

    [Fact]
    public void Add_resolves_dependencies()
    {
        // Arrange
        var (fs, dir, _) = SetupProject();
        var console = new TestConsole();

        // Act — dialog depends on button
        var exitCode = new AddCommand(fs, console).Execute("dialog", overwrite: false, all: false, dryRun: false, dir);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.True(fs.File.Exists(fs.Path.Combine(dir, "Components", "UI", "Button.razor")));
        Assert.True(fs.File.Exists(fs.Path.Combine(dir, "Components", "UI", "Dialog.razor")));
    }

    [Fact]
    public void Add_skips_existing_without_overwrite()
    {
        // Arrange
        var (fs, dir, _) = SetupProject();
        var console = new TestConsole();
        new AddCommand(fs, console).Execute("button", overwrite: false, all: false, dryRun: false, dir);
        var filePath = fs.Path.Combine(dir, "Components", "UI", "Button.razor");
        fs.File.WriteAllText(filePath, "custom content");

        // Act
        var console2 = new TestConsole();
        new AddCommand(fs, console2).Execute("button", overwrite: false, all: false, dryRun: false, dir);

        // Assert — file not overwritten
        Assert.Equal("custom content", fs.File.ReadAllText(filePath));
    }

    [Fact]
    public void Add_overwrites_existing_with_flag()
    {
        // Arrange
        var (fs, dir, _) = SetupProject();
        var console = new TestConsole();
        new AddCommand(fs, console).Execute("button", overwrite: false, all: false, dryRun: false, dir);
        var filePath = fs.Path.Combine(dir, "Components", "UI", "Button.razor");
        fs.File.WriteAllText(filePath, "custom content");

        // Act
        var console2 = new TestConsole();
        new AddCommand(fs, console2).Execute("button", overwrite: true, all: false, dryRun: false, dir);

        // Assert — file overwritten with template
        var content = fs.File.ReadAllText(filePath);
        Assert.Contains("@namespace TestApp.Components.UI", content);
    }

    [Fact]
    public void Add_dry_run_does_not_write_files()
    {
        // Arrange
        var (fs, dir, _) = SetupProject();
        var console = new TestConsole();

        // Act
        var exitCode = new AddCommand(fs, console).Execute("button", overwrite: false, all: false, dryRun: true, dir);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.False(fs.File.Exists(fs.Path.Combine(dir, "Components", "UI", "Button.razor")));
    }

    [Fact]
    public void Add_fails_without_config()
    {
        // Arrange
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/isolated");
        var console = new TestConsole();

        // Act
        var exitCode = new AddCommand(fs, console).Execute("button", overwrite: false, all: false, dryRun: false, "/isolated");

        // Assert
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public void Add_fails_for_unknown_component()
    {
        // Arrange
        var (fs, dir, _) = SetupProject();
        var console = new TestConsole();

        // Act
        var exitCode = new AddCommand(fs, console).Execute("nonexistent", overwrite: false, all: false, dryRun: false, dir);

        // Assert
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public void Add_warns_about_PortalHost_for_overlay_components()
    {
        // Arrange
        var (fs, dir, _) = SetupProject();

        // Create a layout file without PortalHost.
        var layoutDir = fs.Path.Combine(dir, "Components", "Layout");
        fs.Directory.CreateDirectory(layoutDir);
        fs.File.WriteAllText(fs.Path.Combine(layoutDir, "MainLayout.razor"), """
            @inherits LayoutComponentBase
            <main>@Body</main>
            """);

        var console = new TestConsole();

        // Act
        new AddCommand(fs, console).Execute("dialog", overwrite: false, all: false, dryRun: false, dir);

        // Assert
        Assert.Contains("PortalHost", console.Output);
        Assert.Contains("Overlay components require", console.Output);
    }

    [Fact]
    public void Add_no_PortalHost_warning_when_already_present()
    {
        // Arrange
        var (fs, dir, _) = SetupProject();

        var layoutDir = fs.Path.Combine(dir, "Components", "Layout");
        fs.Directory.CreateDirectory(layoutDir);
        fs.File.WriteAllText(fs.Path.Combine(layoutDir, "MainLayout.razor"), """
            @inherits LayoutComponentBase
            <PortalHost />
            <main>@Body</main>
            """);

        var console = new TestConsole();

        // Act
        new AddCommand(fs, console).Execute("dialog", overwrite: false, all: false, dryRun: false, dir);

        // Assert
        Assert.DoesNotContain("Overlay components require", console.Output);
    }

    [Fact]
    public void Add_no_PortalHost_warning_for_non_overlay_components()
    {
        // Arrange
        var (fs, dir, _) = SetupProject();

        var layoutDir = fs.Path.Combine(dir, "Components", "Layout");
        fs.Directory.CreateDirectory(layoutDir);
        fs.File.WriteAllText(fs.Path.Combine(layoutDir, "MainLayout.razor"), """
            @inherits LayoutComponentBase
            <main>@Body</main>
            """);

        var console = new TestConsole();

        // Act
        new AddCommand(fs, console).Execute("button", overwrite: false, all: false, dryRun: false, dir);

        // Assert — button is not an overlay, so no warning
        Assert.DoesNotContain("PortalHost", console.Output);
    }
}

public class ListCommandTests
{
    [Fact]
    public void List_returns_zero_exit_code()
    {
        // Arrange
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/project");
        var console = new TestConsole();

        // Act
        var exitCode = new ListCommand(fs, console).Execute("/project");

        // Assert
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void List_shows_installed_marker()
    {
        // Arrange
        var fs = new MockFileSystem();
        var runner = new FakeProcessRunner();
        var dir = "/project";
        fs.Directory.CreateDirectory(dir);
        fs.File.WriteAllText(fs.Path.Combine(dir, "TestApp.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk.Razor">
              <PropertyGroup>
                <RootNamespace>TestApp</RootNamespace>
              </PropertyGroup>
            </Project>
            """);
        var initConsole = new TestConsole();
        new InitCommand(fs, initConsole, runner).Execute(dir);
        var addConsole = new TestConsole();
        new AddCommand(fs, addConsole).Execute("button", overwrite: false, all: false, dryRun: false, dir);

        var listConsole = new TestConsole();

        // Act
        new ListCommand(fs, listConsole).Execute(dir);

        // Assert — button should show the installed marker
        Assert.Contains("✓", listConsole.Output);
        Assert.Contains("button", listConsole.Output);
    }
}

public class ComponentRegistryTests
{
    [Fact]
    public void Registry_finds_component_case_insensitive()
    {
        // Arrange
        var registry = ComponentRegistry.Load();

        // Act
        var result = registry.Find("Button");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("button", result.Name);
    }

    [Fact]
    public void Registry_resolves_transitive_dependencies()
    {
        // Arrange
        var registry = ComponentRegistry.Load();

        // Act — dialog depends on button
        var resolved = registry.ResolveWithDependencies("dialog");

        // Assert — button comes before dialog
        Assert.Equal(2, resolved.Count);
        Assert.Equal("button", resolved[0].Name);
        Assert.Equal("dialog", resolved[1].Name);
    }

    [Fact]
    public void Registry_resolves_drawer_dependencies()
    {
        // Arrange
        var registry = ComponentRegistry.Load();

        // Act
        var resolved = registry.ResolveWithDependencies("drawer");

        // Assert — button comes before drawer
        Assert.Equal(2, resolved.Count);
        Assert.Equal("button", resolved[0].Name);
        Assert.Equal("drawer", resolved[1].Name);
    }
}
