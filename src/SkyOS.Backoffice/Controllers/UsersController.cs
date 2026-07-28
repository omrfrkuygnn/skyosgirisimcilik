using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Backoffice.ViewModels;
using SkyOS.Infrastructure.Identity;
using SkyOS.Shared.Constants;
using SkyOS.Shared.Localization;

namespace SkyOS.Backoffice.Controllers;

public sealed class UsersController : AdminControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppLocalizer _localizer;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        IAuditLogService auditLogs,
        IAppLocalizer localizer)
        : base(auditLogs)
    {
        _userManager = userManager;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .OrderBy(user => user.DisplayName)
            .ThenBy(user => user.Email)
            .Select(user => new AdminUserListItemViewModel
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return View(users);
    }

    public IActionResult Create() => View(new AdminUserCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserCreateViewModel model, CancellationToken cancellationToken)
    {
        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError(nameof(model.ConfirmPassword), _localizer["Admin.PasswordMismatch"]);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();
        var displayName = model.DisplayName.Trim();
        var existingUser = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (existingUser is not null)
        {
            ModelState.AddModelError(nameof(model.Email), _localizer["Admin.UserEmailExists"]);
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = displayName,
            IsActive = model.IsActive,
        };

        var result = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, IdentitySeeder.AdminRole).ConfigureAwait(false);
        await LogActionAsync(
            AuditActions.CreateAdminUser,
            "ApplicationUser",
            user.Id,
            $"{displayName} ({email})").ConfigureAwait(false);

        TempData["SuccessMessage"] = _localizer["Admin.UserCreated"];
        return RedirectToAction(nameof(Index));
    }
}
