using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazeUI.UI.CLI.Registry;

internal sealed class ComponentRegistry
{
    private readonly List<ComponentDefinition> _components;

    private ComponentRegistry(List<ComponentDefinition> components)
    {
        _components = components;
    }

    public static ComponentRegistry Load()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("BlazeUI.UI.CLI.Registry.components.json")
            ?? throw new InvalidOperationException("Could not find embedded components.json");

        var manifest = JsonSerializer.Deserialize<ComponentManifest>(stream)
            ?? throw new InvalidOperationException("Failed to deserialize components.json");

        return new ComponentRegistry(manifest.Components);
    }

    public IReadOnlyList<ComponentDefinition> All => _components;

    public ComponentDefinition? Find(string name) =>
        _components.Find(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns the component and all its transitive dependencies in installation order
    /// (dependencies first).
    /// </summary>
    public List<ComponentDefinition> ResolveWithDependencies(string name)
    {
        var result = new List<ComponentDefinition>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Resolve(name, result, visited);
        return result;
    }

    private void Resolve(string name, List<ComponentDefinition> result, HashSet<string> visited)
    {
        if (!visited.Add(name)) return;

        var component = Find(name)
            ?? throw new InvalidOperationException($"Unknown component: {name}");

        foreach (var dep in component.Deps)
        {
            Resolve(dep, result, visited);
        }

        result.Add(component);
    }
}

internal sealed class ComponentManifest
{
    [JsonPropertyName("components")]
    public List<ComponentDefinition> Components { get; set; } = [];
}

internal sealed class ComponentDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("files")]
    public List<string> Files { get; set; } = [];

    [JsonPropertyName("deps")]
    public List<string> Deps { get; set; } = [];
}
