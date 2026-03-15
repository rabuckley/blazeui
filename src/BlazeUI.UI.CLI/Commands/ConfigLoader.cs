using System.IO.Abstractions;
using System.Text.Json;

namespace BlazeUI.UI.CLI.Commands;

internal static class ConfigLoader
{
    internal static (BlazeUIConfig? Config, string ConfigPath) LoadConfig(IFileSystem fs, string startDir)
    {
        var dir = startDir;
        while (dir is not null)
        {
            var configPath = fs.Path.Combine(dir, "blazeui.json");
            if (fs.File.Exists(configPath))
            {
                var json = fs.File.ReadAllText(configPath);
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var config = JsonSerializer.Deserialize<BlazeUIConfig>(json, options);
                return (config, configPath);
            }
            dir = fs.Directory.GetParent(dir)?.FullName;
        }
        return (null, fs.Path.Combine(startDir, "blazeui.json"));
    }
}
