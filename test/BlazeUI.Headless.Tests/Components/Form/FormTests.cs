using BlazeUI.Headless.Components.Field;
using BlazeUI.Headless.Components.Form;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Tests.Components.Form;

public class FormTests : BunitContext
{
    [Fact]
    public void RendersDefaultFormTag()
    {
        var cut = Render<FormRoot>();
        Assert.Equal("FORM", cut.Find("form").TagName);
    }

    [Fact]
    public void HasNoValidateByDefault()
    {
        // Arrange / Act
        var cut = Render<FormRoot>();

        // Assert — Base UI always sets noValidate so BlazeUI field validation
        // controls all feedback rather than browser-native constraint popups.
        Assert.NotNull(cut.Find("form[novalidate]"));
    }

    [Fact]
    public void NoValidateFalseRemovesAttribute()
    {
        // Arrange / Act
        var cut = Render<FormRoot>(p => p.Add(c => c.NoValidate, false));

        // Assert
        Assert.Empty(cut.FindAll("form[novalidate]"));
    }

    [Fact]
    public void FormErrorMarksMatchingFieldInvalid()
    {
        // Arrange — provide a server error for "username"
        var errors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["username"] = ["Name is required"],
        };

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<FormRoot>(0);
            builder.AddAttribute(1, nameof(FormRoot.Errors), errors);
            builder.AddAttribute(2, nameof(FormRoot.ChildContent), (RenderFragment)(b =>
            {
                b.OpenComponent<FieldRoot>(0);
                b.AddAttribute(1, nameof(FieldRoot.Name), "username");
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert — the field div should carry data-invalid
        Assert.NotNull(cut.Find("[data-invalid]"));
        Assert.Empty(cut.FindAll("[data-valid]"));
    }

    [Fact]
    public void FieldWithNoMatchingFormErrorIsValid()
    {
        // Arrange — error only for "email", field name is "username"
        var errors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["email"] = ["Email is required"],
        };

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<FormRoot>(0);
            builder.AddAttribute(1, nameof(FormRoot.Errors), errors);
            builder.AddAttribute(2, nameof(FormRoot.ChildContent), (RenderFragment)(b =>
            {
                b.OpenComponent<FieldRoot>(0);
                b.AddAttribute(1, nameof(FieldRoot.Name), "username");
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert
        Assert.NotNull(cut.Find("[data-valid]"));
        Assert.Empty(cut.FindAll("[data-invalid]"));
    }

    [Fact]
    public void FormErrorPopulatesFieldError()
    {
        // Arrange
        var errors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["username"] = ["Name is required"],
        };

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<FormRoot>(0);
            builder.AddAttribute(1, nameof(FormRoot.Errors), errors);
            builder.AddAttribute(2, nameof(FormRoot.ChildContent), (RenderFragment)(b =>
            {
                b.OpenComponent<FieldRoot>(0);
                b.AddAttribute(1, nameof(FieldRoot.Name), "username");
                b.AddAttribute(2, nameof(FieldRoot.ChildContent), (RenderFragment)(b2 =>
                {
                    b2.OpenComponent<FieldError>(0);
                    b2.CloseComponent();
                }));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert — the form-level error message text should be in the FieldError element
        var errorEl = cut.Find("[aria-live]");
        Assert.Contains("Name is required", errorEl.TextContent);
    }

    [Fact]
    public void FieldErrorNotRenderedWhenNoFormError()
    {
        // Arrange — no errors provided

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<FormRoot>(0);
            builder.AddAttribute(1, nameof(FormRoot.ChildContent), (RenderFragment)(b =>
            {
                b.OpenComponent<FieldRoot>(0);
                b.AddAttribute(1, nameof(FieldRoot.Name), "username");
                b.AddAttribute(2, nameof(FieldRoot.ChildContent), (RenderFragment)(b2 =>
                {
                    b2.OpenComponent<FieldError>(0);
                    b2.CloseComponent();
                }));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert — FieldError should not be rendered when valid
        Assert.Empty(cut.FindAll("[aria-live]"));
    }

    [Fact]
    public void ExplicitFieldInvalidOverridesFormErrorAbsence()
    {
        // Arrange — no form errors but field explicitly marked invalid

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<FormRoot>(0);
            builder.AddAttribute(1, nameof(FormRoot.ChildContent), (RenderFragment)(b =>
            {
                b.OpenComponent<FieldRoot>(0);
                b.AddAttribute(1, nameof(FieldRoot.Name), "username");
                b.AddAttribute(2, nameof(FieldRoot.Invalid), (bool?)true);
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert
        Assert.NotNull(cut.Find("[data-invalid]"));
    }

    [Fact]
    public void ValidationModeDefaultsToOnSubmit()
    {
        // Arrange / Act
        var cut = Render<FormRoot>();

        // Assert — the Instance of the rendered component reflects the default.
        Assert.Equal(FormValidationMode.OnSubmit, cut.Instance.ValidationMode);
    }

    [Fact]
    public void FieldStandaloneWithoutFormRootIsValid()
    {
        // Fields used outside a FormRoot should be valid by default — the form
        // context is optional.
        var cut = Render<FieldRoot>();

        Assert.NotNull(cut.Find("[data-valid]"));
        Assert.Empty(cut.FindAll("[data-invalid]"));
    }

    [Fact]
    public void MultipleFormErrorsAllAppearInFieldError()
    {
        // Arrange — multiple errors for a single field
        var errors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["email"] = ["Must be a valid email", "Required"],
        };

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<FormRoot>(0);
            builder.AddAttribute(1, nameof(FormRoot.Errors), errors);
            builder.AddAttribute(2, nameof(FormRoot.ChildContent), (RenderFragment)(b =>
            {
                b.OpenComponent<FieldRoot>(0);
                b.AddAttribute(1, nameof(FieldRoot.Name), "email");
                b.AddAttribute(2, nameof(FieldRoot.ChildContent), (RenderFragment)(b2 =>
                {
                    b2.OpenComponent<FieldError>(0);
                    b2.CloseComponent();
                }));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert — both error messages should appear
        var errorEl = cut.Find("[aria-live]");
        Assert.Contains("Must be a valid email", errorEl.TextContent);
        Assert.Contains("Required", errorEl.TextContent);
    }
}
