using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Dialog;

internal sealed class DialogContext
{
    public bool Open { get; set; }
    public bool Modal { get; set; } = true;

    /// <summary>
    /// ARIA role for the popup element. <c>"dialog"</c> for regular dialogs,
    /// <c>"alertdialog"</c> for alert dialogs. Base UI shares all Dialog
    /// sub-components between Dialog and AlertDialog; the role is the
    /// only difference the popup needs to know about.
    /// </summary>
    public string Role { get; set; } = "dialog";

    /// <summary>
    /// 1-based nesting depth. The outermost dialog is level 1.
    /// Used by <see cref="DialogBackdrop"/> to suppress rendering
    /// when nested (only the root backdrop renders by default).
    /// </summary>
    public int NestingLevel { get; set; } = 1;
    public string? TitleId { get; set; }
    public string? DescriptionId { get; set; }
    public string PopupId { get; set; } = "";
    public Func<bool, Task> SetOpen { get; set; } = _ => Task.CompletedTask;
    public Func<Task> Close { get; set; } = () => Task.CompletedTask;
    public IJSObjectReference? JsModule { get; set; }
    /// <summary>
    /// DotNetObjectReference for JS interop callbacks. Typed as <c>object</c>
    /// because both <see cref="DialogRoot"/> and
    /// <see cref="AlertDialog.AlertDialogRoot"/> set this field with their
    /// own generic DotNetObjectReference.
    /// </summary>
    public object? DotNetRef { get; set; }

}
