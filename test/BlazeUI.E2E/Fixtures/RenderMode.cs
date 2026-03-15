namespace BlazeUI.E2E.Fixtures;

public enum RenderMode
{
    Server,
    WebAssembly,
}

public static class RenderModes
{
    public static TheoryData<RenderMode> All => new()
    {
        RenderMode.Server,
        RenderMode.WebAssembly,
    };
}
