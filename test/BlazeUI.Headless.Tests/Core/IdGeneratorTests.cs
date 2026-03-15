using BlazeUI.Headless.Core;

namespace BlazeUI.Headless.Tests.Core;

public class IdGeneratorTests
{
    [Fact]
    public void Next_ReturnsIdWithCorrectPrefix()
    {
        var id = IdGenerator.Next("dialog");
        Assert.StartsWith("blazeui-dialog-", id);
    }

    [Fact]
    public void Next_ReturnsUniqueIds()
    {
        var ids = Enumerable.Range(0, 100)
            .Select(_ => IdGenerator.Next("test"))
            .ToList();

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Next_IsThreadSafe()
    {
        var ids = new System.Collections.Concurrent.ConcurrentBag<string>();

        Parallel.For(0, 1000, _ =>
        {
            ids.Add(IdGenerator.Next("parallel"));
        });

        Assert.Equal(1000, ids.Distinct().Count());
    }
}
