using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace YamanCam.Web.Controllers;

public class LicenseController : Controller
{
    private readonly ICenterLicenseConnectionService _licenseService;
    private readonly IAppLogService _appLogService;

    public LicenseController(ICenterLicenseConnectionService licenseService, IAppLogService appLogService)
    {
        _licenseService = licenseService;
        _appLogService = appLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Status()
    {
        var status = await _licenseService.RefreshStatusIfNeededAsync();
        ViewBag.LicenseStatus = status;
        return View(new LicenseRequestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(LicenseRequestViewModel vm)
    {
        var status = await _licenseService.RefreshStatusIfNeededAsync();
        if (status.HasRequest)
        {
            await _appLogService.WriteInfoAsync("License", "RequestSkipped", "Mevcut lisans talebi oldugu icin yeni talep atlandi.");
            TempData["ErrorMessage"] = "Bu sunucu icin zaten lisans talebi bulunuyor.";
            return RedirectToAction(nameof(Status));
        }

        if (!ModelState.IsValid)
        {
            await _appLogService.WriteErrorAsync("License", "ValidationFailed", "Lisans talep formu dogrulama hatasi.");
            ViewBag.LicenseStatus = status;
            return View("Status", vm);
        }

        const int roomCount = 1;
        var result = await _licenseService.CreateLicenseRequestAsync(new LicenseRequestData
        {
            HotelName = vm.HotelName.Trim(),
            CompanyName = vm.CompanyName.Trim(),
            RoomCount = roomCount,
            LicenceUser = vm.LicenceUser.Trim()
        });

        if (!result.Success)
        {
            await _appLogService.WriteErrorAsync("License", "RequestFailed", result.Message);
            ModelState.AddModelError(string.Empty, result.Message);
            ViewBag.LicenseStatus = status;
            return View("Status", vm);
        }

        await _appLogService.WriteInfoAsync("License", "RequestCreated", $"Lisans talebi olusturuldu. RequestId={result.RequestId}");
        TempData["SuccessMessage"] = $"{result.Message} Talep No: {result.RequestId}";
        return RedirectToAction(nameof(Status));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve()
    {
        var status = await _licenseService.RefreshStatusNowAsync();
        if (status.IsActive)
        {
            await _appLogService.WriteInfoAsync("License", "Approved", "Lisans onayi basarili.");
            TempData["SuccessMessage"] = "Onay alindi. Lisans aktif.";
        }
        else
        {
            await _appLogService.WriteErrorAsync("License", "ApproveFailed", "Lisans onayi basarisiz veya beklemede.");
            TempData["ErrorMessage"] = "Merkezde IsActive henuz 1 degil veya kayit bulunamadi.";
        }

        return RedirectToAction(nameof(Status));
    }
}
