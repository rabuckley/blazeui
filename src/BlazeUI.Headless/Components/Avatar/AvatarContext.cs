namespace BlazeUI.Headless.Components.Avatar;

internal sealed class AvatarContext
{
    public AvatarLoadingStatus Status { get; set; } = AvatarLoadingStatus.Idle;
    private readonly Func<Task> _onChanged;

    public AvatarContext(Func<Task> onChanged)
    {
        _onChanged = onChanged;
    }

    public Task SetStatusAsync(AvatarLoadingStatus status)
    {
        Status = status;
        return _onChanged();
    }
}
