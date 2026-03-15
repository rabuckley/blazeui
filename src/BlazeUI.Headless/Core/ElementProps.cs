using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Core;

/// <summary>
/// Carries the merged attribute dictionary and child content to a custom <see cref="BlazeElement{TState}.Render"/> delegate,
/// allowing consumers to replace the root element while retaining all headless behavior via <c>@attributes</c>.
/// </summary>
public readonly record struct ElementProps(
    IReadOnlyDictionary<string, object> Attributes,
    RenderFragment? ChildContent);
