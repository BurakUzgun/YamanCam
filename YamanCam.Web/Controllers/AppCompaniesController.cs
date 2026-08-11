using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

public class AppCompaniesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;

    public AppCompaniesController(ApplicationDbContext context, IAppLogService appLogService)
    {
        _context = context;
        _appLogService = appLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var companies = await _context.AppCompanies
            .AsNoTracking()
            .OrderBy(x => x.CompanyCode)
            .Select(x => new AppCompanyEditViewModel
            {
                RecId = x.RecId,
                CompanyCode = x.CompanyCode ?? string.Empty,
                CompanyName = x.CompanyName ?? string.Empty,
                TaxNumber = x.TaxNumber,
                TaxOffice = x.TaxOffice,
                City = x.City,
                Phone = x.Phone,
                Email = x.Email,
                IsActive = x.IsActive != false
            })
            .ToListAsync();

        return View(companies);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        return View("Edit", new AppCompanyEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppCompanyEditViewModel vm)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        NormalizeCheckboxes(vm);
        await ValidateCompanyAsync(vm);

        if (!ModelState.IsValid)
        {
            return View("Edit", vm);
        }

        var company = MapToEntity(vm);
        company.CreatedDate = DateTime.UtcNow;
        _context.AppCompanies.Add(company);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "Company",
            "Created",
            $"Şirket oluşturuldu. Code={company.CompanyCode}",
            newValue: BuildAuditValue(company));

        TempData["SuccessMessage"] = "Şirket kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var company = await _context.AppCompanies.AsNoTracking().FirstOrDefaultAsync(x => x.RecId == id);
        if (company is null)
        {
            return NotFound();
        }

        return View(MapToViewModel(company));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppCompanyEditViewModel vm)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        if (id != vm.RecId)
        {
            return NotFound();
        }

        NormalizeCheckboxes(vm);
        var company = await _context.AppCompanies.FirstOrDefaultAsync(x => x.RecId == id);
        if (company is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(company);
        await ValidateCompanyAsync(vm);

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        ApplyViewModel(company, vm);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "Company",
            "Updated",
            $"Şirket güncellendi. Code={company.CompanyCode}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(company));

        TempData["SuccessMessage"] = "Şirket güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    private IActionResult? EnsureAdmin()
    {
        if (!string.Equals(User.FindFirst("IsRight")?.Value, "true", StringComparison.Ordinal))
        {
            return RedirectToAction("Index", "Home");
        }

        return null;
    }

    private void NormalizeCheckboxes(AppCompanyEditViewModel vm)
    {
        vm.IsActive = string.Equals(Request.Form["IsActive"], "true", StringComparison.Ordinal);
    }

    private async Task ValidateCompanyAsync(AppCompanyEditViewModel vm)
    {
        var codeExists = await _context.AppCompanies.AnyAsync(x =>
            x.CompanyCode == vm.CompanyCode.Trim() && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.CompanyCode), "Bu şirket kodu zaten kullanılıyor.");
        }
    }

    private static AppCompanyEditViewModel MapToViewModel(AppCompany company)
    {
        return new AppCompanyEditViewModel
        {
            RecId = company.RecId,
            CompanyCode = company.CompanyCode ?? string.Empty,
            CompanyName = company.CompanyName ?? string.Empty,
            TaxNumber = company.TaxNumber,
            TaxOffice = company.TaxOffice,
            Address = company.Address,
            City = company.City,
            Phone = company.Phone,
            Email = company.Email,
            IsActive = company.IsActive != false
        };
    }

    private static AppCompany MapToEntity(AppCompanyEditViewModel vm)
    {
        var company = new AppCompany();
        ApplyViewModel(company, vm);
        return company;
    }

    private static void ApplyViewModel(AppCompany company, AppCompanyEditViewModel vm)
    {
        company.CompanyCode = vm.CompanyCode.Trim();
        company.CompanyName = vm.CompanyName.Trim();
        company.TaxNumber = vm.TaxNumber?.Trim();
        company.TaxOffice = vm.TaxOffice?.Trim();
        company.Address = vm.Address?.Trim();
        company.City = vm.City?.Trim();
        company.Phone = vm.Phone?.Trim();
        company.Email = vm.Email?.Trim();
        company.IsActive = vm.IsActive;
    }

    private static string BuildAuditValue(AppCompany company)
    {
        var sb = new StringBuilder();
        sb.Append($"Code={company.CompanyCode}, Name={company.CompanyName}, TaxNumber={company.TaxNumber ?? "-"}");
        sb.Append($", City={company.City ?? "-"}, IsActive={company.IsActive == true}");
        return sb.ToString();
    }
}
