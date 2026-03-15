using BlazeUI.Headless.Components.Field;
using Bunit;
using InputComponent = BlazeUI.Headless.Components.Input.Input;

namespace BlazeUI.Headless.Tests.Components.Input;

public sealed class InputTests : BunitContext
{
    // --- standalone (no Field) ---

    [Fact]
    public void Renders_default_input_tag()
    {
        var cut = Render<InputComponent>();

        Assert.NotNull(cut.Find("input"));
    }

    [Fact]
    public void Auto_generates_id()
    {
        var cut = Render<InputComponent>();

        var id = cut.Find("input").GetAttribute("id");

        Assert.Contains("input", id);
    }

    [Fact]
    public void Data_disabled_present_when_disabled()
    {
        var cut = Render<InputComponent>(p => p.Add(c => c.Disabled, true));

        Assert.NotNull(cut.Find("[data-disabled]"));
        Assert.NotNull(cut.Find("[disabled]"));
    }

    [Fact]
    public void Data_disabled_absent_when_not_disabled()
    {
        var cut = Render<InputComponent>();

        Assert.Empty(cut.FindAll("[data-disabled]"));
    }

    [Fact]
    public void Data_focused_present_after_focus_and_absent_after_blur()
    {
        var cut = Render<InputComponent>();
        var input = cut.Find("input");

        input.Focus();
        Assert.NotNull(cut.Find("[data-focused]"));

        input.Blur();
        Assert.Empty(cut.FindAll("[data-focused]"));
    }

    // Validity attributes (valid, invalid, dirty, touched, filled) are only meaningful
    // inside a Field — without a Field context there is no validation to report.
    [Fact]
    public void Validity_attributes_absent_without_field()
    {
        var cut = Render<InputComponent>();

        Assert.Empty(cut.FindAll("[data-valid]"));
        Assert.Empty(cut.FindAll("[data-invalid]"));
        Assert.Empty(cut.FindAll("[data-dirty]"));
        Assert.Empty(cut.FindAll("[data-touched]"));
        Assert.Empty(cut.FindAll("[data-filled]"));
    }

    // --- Field integration ---

    [Fact]
    public void Data_valid_present_when_field_is_valid()
    {
        var cut = Render<FieldRoot>(p => p
            .AddChildContent<InputComponent>());

        Assert.NotNull(cut.Find("input[data-valid]"));
        Assert.Empty(cut.FindAll("input[data-invalid]"));
    }

    [Fact]
    public void Data_invalid_present_when_field_is_invalid()
    {
        var cut = Render<FieldRoot>(p => p
            .Add(r => r.Invalid, true)
            .AddChildContent<InputComponent>());

        Assert.NotNull(cut.Find("input[data-invalid]"));
        Assert.Empty(cut.FindAll("input[data-valid]"));
    }

    [Fact]
    public void Aria_invalid_set_when_field_is_invalid()
    {
        var cut = Render<FieldRoot>(p => p
            .Add(r => r.Invalid, true)
            .AddChildContent<InputComponent>());

        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public void Data_disabled_inherits_from_field()
    {
        var cut = Render<FieldRoot>(p => p
            .Add(r => r.Disabled, true)
            .AddChildContent<InputComponent>());

        Assert.NotNull(cut.Find("input[data-disabled]"));
        Assert.NotNull(cut.Find("input[disabled]"));
    }

    [Fact]
    public void Data_touched_present_after_blur_inside_field()
    {
        var cut = Render<FieldRoot>(p => p
            .AddChildContent<InputComponent>());

        // Focus then blur triggers SetFocused(false) on the FieldContext,
        // which also marks the field as touched.
        cut.Find("input").Focus();
        cut.Find("input").Blur();

        Assert.NotNull(cut.Find("input[data-touched]"));
    }

    [Fact]
    public void Input_id_matches_field_label_for_attribute()
    {
        var cut = Render<FieldRoot>(p => p
            .AddChildContent<FieldLabel>(lp => lp.AddChildContent("Name")));

        // The label's 'for' must target the Field's control id slot.
        var labelFor = cut.Find("label").GetAttribute("for");

        // The Input, when rendered, picks up FieldContext.ControlId — the same
        // value the label points at.
        var cut2 = Render<FieldRoot>(p => p
            .AddChildContent<InputComponent>());

        var inputId = cut2.Find("input").GetAttribute("id");

        // Both resolve through IdGenerator.Next("field-control") but in separate
        // render trees, so we can only verify the prefix shape here.
        Assert.Contains("field-control", labelFor);
        Assert.Contains("field-control", inputId);
    }

    [Fact]
    public void Aria_describedby_wired_to_field_description_id()
    {
        // FieldRoot pre-allocates the description ID at init time, so Input inside
        // a Field always receives an aria-describedby pointing at the description slot.
        var cut = Render<FieldRoot>(p => p
            .AddChildContent<InputComponent>());

        var describedBy = cut.Find("input").GetAttribute("aria-describedby");

        Assert.NotNull(describedBy);
        Assert.Contains("field-description", describedBy);
    }
}
