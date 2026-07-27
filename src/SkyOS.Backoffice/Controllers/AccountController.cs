using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Backoffice.ViewModels;
using SkyOS.Infrastructure.Identity;
using SkyOS.Shared.Constants;
using SkyOS.Shared.Localization;

namespace SkyOS.Backoffice.Controllers;

public sealed class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _auditLogs;
    private readonly IAppLocalizer _localizer;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IAuditLogService auditLogs,
        IAppLocalizer localizer)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _auditLogs = auditLogs;
        _localizer = localizer;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
        if (user is null || !user.IsActive)
        {
            await WriteFailedLoginAsync(model.Email, cancellationToken).ConfigureAwait(false);
            ModelState.AddModelError(string.Empty, _localizer["Admin.InvalidCredentials"]);
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            await WriteFailedLoginAsync(model.Email, cancellationToken).ConfigureAwait(false);
            ModelState.AddModelError(string.Empty, _localizer["Admin.InvalidCredentials"]);
            return View(model);
        }

        await _auditLogs.WriteAsync(new Application.DTOs.Admin.AuditLogWriteDto
        {
            UserId = user.Id,
            UserEmail = user.Email ?? model.Email,
            Action = AuditActions.Login,
            Details = _localizer["Admin.AuditDetail.LoginSuccess"],
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
        }, cancellationToken).ConfigureAwait(false);

        return LocalRedirect(string.IsNullOrWhiteSpace(model.ReturnUrl) ? "/Dashboard" : model.ReturnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _auditLogs.WriteAsync(new Application.DTOs.Admin.AuditLogWriteDto
        {
            UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            UserEmail = User.Identity?.Name ?? string.Empty,
            Action = AuditActions.Logout,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
        }, cancellationToken).ConfigureAwait(false);

        await _signInManager.SignOutAsync().ConfigureAwait(false);
        return RedirectToAction(nameof(Login));
    }

    private Task WriteFailedLoginAsync(string email, CancellationToken cancellationToken) =>
        _auditLogs.WriteAsync(new Application.DTOs.Admin.AuditLogWriteDto
        {
            UserEmail = email,
            Action = AuditActions.LoginFailed,
            Details = _localizer["Admin.AuditDetail.LoginFailed"],
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
        }, cancellationToken);
}
