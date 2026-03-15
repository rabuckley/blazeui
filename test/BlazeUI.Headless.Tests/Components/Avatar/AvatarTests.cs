using BlazeUI.Headless.Components.Avatar;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Tests.Components.Avatar;

public class AvatarTests : BunitContext
{
    // -- AvatarRoot --

    [Fact]
    public void Root_RendersSpanByDefault()
    {
        var cut = Render<AvatarRoot>();
        Assert.Equal("SPAN", cut.Find("span").TagName);
    }

    [Fact]
    public void Root_HasIdleStatusDataAttributeByDefault()
    {
        var cut = Render<AvatarRoot>();
        Assert.Equal("idle", cut.Find("span").GetAttribute("data-status"));
    }

    // -- AvatarFallback visibility --

    [Fact]
    public void Fallback_RendersWhenStatusIsIdle()
    {
        var ctx = new AvatarContext(() => Task.CompletedTask) { Status = AvatarLoadingStatus.Idle };
        var cut = Render<CascadingValue<AvatarContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AvatarFallback>(f => f.AddChildContent("AC")));

        Assert.Contains("AC", cut.Markup);
    }

    [Fact]
    public void Fallback_RendersWhenStatusIsError()
    {
        var ctx = new AvatarContext(() => Task.CompletedTask) { Status = AvatarLoadingStatus.Error };
        var cut = Render<CascadingValue<AvatarContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AvatarFallback>(f => f.AddChildContent("AC")));

        Assert.Contains("AC", cut.Markup);
    }

    [Fact]
    public void Fallback_RendersWhenStatusIsLoading()
    {
        // Base UI keeps the fallback visible while the image is loading to avoid
        // a flash when images load slowly.
        var ctx = new AvatarContext(() => Task.CompletedTask) { Status = AvatarLoadingStatus.Loading };
        var cut = Render<CascadingValue<AvatarContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AvatarFallback>(f => f.AddChildContent("AC")));

        Assert.Contains("AC", cut.Markup);
    }

    [Fact]
    public void Fallback_NotRenderedWhenStatusIsLoaded()
    {
        var ctx = new AvatarContext(() => Task.CompletedTask) { Status = AvatarLoadingStatus.Loaded };
        var cut = Render<CascadingValue<AvatarContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AvatarFallback>(f => f.AddChildContent("AC")));

        Assert.DoesNotContain("AC", cut.Markup);
    }

    [Fact]
    public void Fallback_HasCorrectDataStatusAttribute()
    {
        var ctx = new AvatarContext(() => Task.CompletedTask) { Status = AvatarLoadingStatus.Error };
        var cut = Render<CascadingValue<AvatarContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AvatarFallback>());

        Assert.Equal("error", cut.Find("span").GetAttribute("data-status"));
    }

    [Fact]
    public void Fallback_ZeroDelayShowsImmediately()
    {
        var ctx = new AvatarContext(() => Task.CompletedTask) { Status = AvatarLoadingStatus.Idle };
        var cut = Render<CascadingValue<AvatarContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AvatarFallback>(f => f
                .Add(c => c.Delay, 0)
                .AddChildContent("AC")));

        Assert.Contains("AC", cut.Markup);
    }

    // -- AvatarImage visibility --

    [Fact]
    public void Image_NotRenderedWhenNoSrcAndStatusIsIdle()
    {
        // Without a src the image element has nothing to load, so it should not be
        // mounted regardless of the context status.
        var ctx = new AvatarContext(() => Task.CompletedTask) { Status = AvatarLoadingStatus.Idle };
        var cut = Render<CascadingValue<AvatarContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AvatarImage>());

        Assert.Empty(cut.FindAll("img"));
    }

    [Fact]
    public void Image_NotRenderedWhenStatusIsError()
    {
        var ctx = new AvatarContext(() => Task.CompletedTask) { Status = AvatarLoadingStatus.Error };
        var cut = Render<CascadingValue<AvatarContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AvatarImage>(img => img.Add(c => c.Src, "avatar.png")));

        Assert.Empty(cut.FindAll("img"));
    }

    [Fact]
    public void Image_RenderedWhenStatusIsLoading()
    {
        var ctx = new AvatarContext(() => Task.CompletedTask) { Status = AvatarLoadingStatus.Loading };
        var cut = Render<CascadingValue<AvatarContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AvatarImage>(img => img.Add(c => c.Src, "avatar.png")));

        Assert.NotEmpty(cut.FindAll("img"));
    }

    [Fact]
    public void Image_RenderedWhenStatusIsLoaded()
    {
        var ctx = new AvatarContext(() => Task.CompletedTask) { Status = AvatarLoadingStatus.Loaded };
        var cut = Render<CascadingValue<AvatarContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AvatarImage>(img => img.Add(c => c.Src, "avatar.png")));

        Assert.NotEmpty(cut.FindAll("img"));
    }

    [Fact]
    public void Image_HasDataStatusAttribute()
    {
        var ctx = new AvatarContext(() => Task.CompletedTask) { Status = AvatarLoadingStatus.Loading };
        var cut = Render<CascadingValue<AvatarContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AvatarImage>(img => img.Add(c => c.Src, "avatar.png")));

        Assert.Equal("loading", cut.Find("img").GetAttribute("data-status"));
    }

    [Fact]
    public void Image_SetsSrcAttribute()
    {
        var ctx = new AvatarContext(() => Task.CompletedTask) { Status = AvatarLoadingStatus.Loading };
        var cut = Render<CascadingValue<AvatarContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AvatarImage>(img => img.Add(c => c.Src, "avatar.png")));

        Assert.Equal("avatar.png", cut.Find("img").GetAttribute("src"));
    }

    [Fact]
    public void Image_SetsAltAttribute()
    {
        var ctx = new AvatarContext(() => Task.CompletedTask) { Status = AvatarLoadingStatus.Loading };
        var cut = Render<CascadingValue<AvatarContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AvatarImage>(img => img
                .Add(c => c.Src, "avatar.png")
                .Add(c => c.Alt, "User avatar")));

        Assert.Equal("User avatar", cut.Find("img").GetAttribute("alt"));
    }

    // -- Integration: Root + Image + Fallback --

    [Fact]
    public void Root_UpdatesDataStatusWhenImageParametersSet()
    {
        // AvatarImage.OnParametersSetAsync transitions the root to 'loading' when
        // a src is provided and the status is currently idle.
        var cut = Render<AvatarRoot>(p => p
            .AddChildContent<AvatarImage>(img => img.Add(c => c.Src, "avatar.png")));

        var root = cut.Find("span");
        Assert.Equal("loading", root.GetAttribute("data-status"));
    }
}
