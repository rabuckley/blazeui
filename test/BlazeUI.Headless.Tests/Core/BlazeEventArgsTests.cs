using BlazeUI.Headless.Core;

namespace BlazeUI.Headless.Tests.Core;

public class BlazeEventArgsTests
{
    [Fact]
    public void Constructor_SetsValueAndReason()
    {
        var args = new BlazeEventArgs<bool>(true, "escape-key");

        Assert.True(args.Value);
        Assert.Equal("escape-key", args.Reason);
    }

    [Fact]
    public void Cancel_SetsIsCanceled()
    {
        var args = new BlazeEventArgs<int>(1);
        Assert.False(args.IsCanceled);

        args.Cancel();

        Assert.True(args.IsCanceled);
    }

    [Fact]
    public void AllowPropagation_SetsShouldPropagate()
    {
        var args = new BlazeEventArgs<string>("test");
        Assert.False(args.ShouldPropagate);

        args.AllowPropagation();

        Assert.True(args.ShouldPropagate);
    }

    [Fact]
    public void DefaultState_IsNotCanceledAndNotPropagating()
    {
        var args = new BlazeEventArgs<string>("value");

        Assert.False(args.IsCanceled);
        Assert.False(args.ShouldPropagate);
    }
}
