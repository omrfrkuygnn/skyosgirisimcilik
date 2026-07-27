using Microsoft.AspNetCore.Mvc;
using SkyOS.Application.DTOs.Milestones;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Domain.Enums;
using SkyOS.Web.ViewModels;

namespace SkyOS.Web.Controllers;

/// <summary>
/// Serves the mostly-static institutional pages. Data-driven sections (team, milestones)
/// are resolved through Application services — never the DbContext directly.
/// </summary>
public sealed class PagesController : Controller
{
    private readonly ITeamService _teamService;
    private readonly IMilestoneService _milestoneService;

    public PagesController(ITeamService teamService, IMilestoneService milestoneService)
    {
        _teamService = teamService;
        _milestoneService = milestoneService;
    }

    [HttpGet]
    public IActionResult Hakkimizda() => View();

    [HttpGet]
    public IActionResult Urun() => View();

    [HttpGet]
    public IActionResult KullanimAlanlari() => View();

    [HttpGet]
    public IActionResult Yatirimcilar() => View();

    [HttpGet]
    public IActionResult GizlilikPolitikasi() => View();

    [HttpGet]
    public async Task<IActionResult> Ekip(CancellationToken cancellationToken)
    {
        var all = await _teamService.GetAllAsync(cancellationToken);
        var viewModel = new TeamPageViewModel
        {
            Leaders = all.Where(m => m.IsLeader).ToList(),
            Members = all.Where(m => !m.IsLeader).ToList(),
        };
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Basarilar(CancellationToken cancellationToken)
    {
        var all = await _milestoneService.GetAllAsync(cancellationToken);
        var grouped = all
            .GroupBy(m => m.Category)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<MilestoneDto>)g.ToList());

        var viewModel = new AchievementsPageViewModel
        {
            MilestonesByCategory = grouped,
        };
        return View(viewModel);
    }
}
