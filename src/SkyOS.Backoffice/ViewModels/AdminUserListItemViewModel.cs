namespace SkyOS.Backoffice.ViewModels;

public sealed class AdminUserListItemViewModel
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
