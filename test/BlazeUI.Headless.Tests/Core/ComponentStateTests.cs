using BlazeUI.Headless.Core;

namespace BlazeUI.Headless.Tests.Core;

public class ComponentStateTests
{
    [Fact]
    public void Value_ReturnsDefaultWhenUncontrolled()
    {
        var state = new ComponentState<bool>(true);
        Assert.True(state.Value);
    }

    [Fact]
    public void SetInternal_UpdatesValueWhenUncontrolled()
    {
        var state = new ComponentState<int>(0);

        state.SetInternal(42);

        Assert.Equal(42, state.Value);
    }

    [Fact]
    public void SetControlled_OverridesInternalValue()
    {
        var state = new ComponentState<string>("default");
        state.SetInternal("internal");

        state.SetControlled("controlled");

        Assert.Equal("controlled", state.Value);
    }

    [Fact]
    public void SetInternal_IsIgnoredWhenControlled()
    {
        var state = new ComponentState<int>(0);
        state.SetControlled(10);

        state.SetInternal(99);

        Assert.Equal(10, state.Value);
    }

    [Fact]
    public void ClearControlled_RevertsToInternalValue()
    {
        var state = new ComponentState<int>(0);
        state.SetInternal(5);
        state.SetControlled(10);

        state.ClearControlled();

        Assert.Equal(5, state.Value);
    }
}
