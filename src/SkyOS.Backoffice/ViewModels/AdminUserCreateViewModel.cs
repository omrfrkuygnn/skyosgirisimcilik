using System.ComponentModel.DataAnnotations;

namespace SkyOS.Backoffice.ViewModels;

public sealed class AdminUserCreateViewModel
{
    [Required]
    [StringLength(120)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
