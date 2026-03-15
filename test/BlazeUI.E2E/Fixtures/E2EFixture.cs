using System.Diagnostics;
using System.Net;

namespace BlazeUI.E2E.Fixtures;

public class E2EFixture : IAsyncLifetime
{
    private Process? _serverHost;
    private Process? _wasmHost;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Fixture not initialized.");

    private const string ServerBaseUrl = "http://127.0.0.1:5199";
    private const string WasmBaseUrl = "http://127.0.0.1:5299";

    // Resolve project paths relative to the repository root.
    // The test assembly runs from bin/Debug/net10.0/, so walk up to find the repo root.
    private static string RepoRoot
    {
        get
        {
            var dir = AppContext.BaseDirectory;
            while (dir is not null && !File.Exists(Path.Combine(dir, "BlazeUI.slnx")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            return dir ?? throw new InvalidOperationException("Could not find repository root (BlazeUI.slnx).");
        }
    }

    private static readonly string ServerProjectPath =
        Path.Combine(RepoRoot, "test", "BlazeUI.E2E.Host.Server");

    private static readonly string WasmProjectPath =
        Path.Combine(RepoRoot, "test", "BlazeUI.E2E.Host.Wasm", "BlazeUI.E2E.Host.Wasm");

    public async ValueTask InitializeAsync()
    {
        _serverHost = StartHost(ServerProjectPath, 5199);
        _wasmHost = StartHost(WasmProjectPath, 5299);

        await Task.WhenAll(
            WaitForHostAsync(ServerBaseUrl),
            WaitForHostAsync(WasmBaseUrl));

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
    }

    public async Task<IPage> CreatePageAsync(RenderMode mode)
    {
        var context = await Browser.NewContextAsync();
        return await context.NewPageAsync();
    }

    public static string BaseUrlFor(RenderMode mode) => mode switch
    {
        RenderMode.Server => ServerBaseUrl,
        RenderMode.WebAssembly => WasmBaseUrl,
        _ => throw new ArgumentOutOfRangeException(nameof(mode)),
    };

    /// <summary>
    /// Navigates to a page and waits for the Blazor interactive runtime to complete
    /// the SSR→interactive handoff. The <see cref="BlazeUI.E2E.Pages.Layout.InteractiveMarker"/>
    /// component sets <c>window.__blazeInteractive = true</c> on its first interactive render.
    /// </summary>
    public static async Task NavigateAndWaitForInteractiveAsync(
        IPage page, string path, RenderMode mode)
    {
        var baseUrl = BaseUrlFor(mode);
        await page.GotoAsync(
            $"{baseUrl}{path}",
            new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        await page.WaitForFunctionAsync(
            "() => window.__blazeInteractive === true",
            null,
            new() { Timeout = 30_000 });
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
            await _browser.DisposeAsync();
        _playwright?.Dispose();

        StopHost(_serverHost);
        StopHost(_wasmHost);
    }

    // Detect whether the test assembly was built in Release or Debug
    // so the host projects use the matching configuration.
    private static readonly string BuildConfiguration =
        AppContext.BaseDirectory.Contains("Release") ? "Release" : "Debug";

    private static Process StartHost(string projectPath, int port)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectPath}\" --no-build -c {BuildConfiguration} --urls http://127.0.0.1:{port}",
                Environment =
                {
                    ["ASPNETCORE_ENVIRONMENT"] = "Development",
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        process.Start();

        // Drain stdout/stderr to avoid deadlocks from full OS pipe buffers.
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }

    private static async Task WaitForHostAsync(string baseUrl, int timeoutSeconds = 60)
    {
        using var client = new HttpClient();
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync(baseUrl);
                if (response.StatusCode is not HttpStatusCode.ServiceUnavailable)
                    return;
            }
            catch (HttpRequestException)
            {
                // Host not ready yet.
            }

            await Task.Delay(250);
        }

        throw new TimeoutException($"Host at {baseUrl} did not become ready within {timeoutSeconds}s.");
    }

    private static void StopHost(Process? process)
    {
        if (process is null || process.HasExited)
            return;

        process.Kill(entireProcessTree: true);
        process.Dispose();
    }
}
