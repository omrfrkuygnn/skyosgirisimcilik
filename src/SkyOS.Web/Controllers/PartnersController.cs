using Microsoft.AspNetCore.Mvc;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Web.ViewModels;

namespace SkyOS.Web.Controllers;

public sealed class PartnersController : Controller
{
    private readonly IPartnerService _partnerService;

    public PartnersController(IPartnerService partnerService) => _partnerService = partnerService;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var all = await _partnerService.GetAllAsync(cancellationToken);
        var viewModel = new PartnersPageViewModel
        {
            VerifiedPartners = all.Where(p => p.IsVerifiedRelationship).ToList(),
            OtherPartners = all.Where(p => !p.IsVerifiedRelationship).ToList(),
        };
        return View(viewModel);
    }
}
