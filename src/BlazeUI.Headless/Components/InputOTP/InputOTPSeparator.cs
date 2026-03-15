using BlazeUI.Headless.Core;

namespace BlazeUI.Headless.Components.InputOTP;

public class InputOTPSeparator : BlazeElement<bool>
{
    protected override string DefaultTag => "div";
    protected override bool GetCurrentState() => false;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("role", "separator");
    }
}
