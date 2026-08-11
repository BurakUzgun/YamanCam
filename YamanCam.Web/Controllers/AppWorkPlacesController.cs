using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

public class AppWorkPlacesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;

    public AppWorkPlacesController(ApplicationDbContext context, IAppLogService appLogService)
    {
        _context = context;
        _appLogService = appLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? companyId)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var vm = new AppWorkPlaceListViewModel
        {
            CompanyId = companyId,
            CompanyOptions = await LoadCompanyOptionsAsync()
        };

        var query = _context.AppWorkPlaces.AsNoTracking().AsQueryable();
        if (companyId.HasValue && companyId.Value > 0)
        {
            query = query.Where(x => x.CompanyId == companyId.Value);
        }

        vm.Items = await query
            .OrderBy(x => x.CompanyId)
            .ThenBy(x => x.WorkPlaceCode)
            .Join(
                _context.AppCompanies.AsNoTracking(),
                wp => wp.CompanyId,
                c => c.RecId,
                (wp, c) => new AppWorkPlaceListItemViewModel
                {
                    RecId = wp.RecId,
                    CompanyId = wp.CompanyId,
                    CompanyName = c.CompanyName,
                    WorkPlaceCode = wp.WorkPlaceCode,
                    WorkPlaceName = wp.WorkPlaceName,
                    City = wp.City,
                    IsDefault = wp.IsDefault,
                    IsActive = wp.IsActive
                })
            .ToListAsync();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? companyId)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var vm = new AppWorkPlaceEditViewModel
        {
            CompanyId = companyId ?? 0,
            CompanyOptions = await LoadCompanyOptionsAsync()
        };

        if (vm.CompanyId == 0 && vm.CompanyOptions.Count == 1)
        {
            vm.CompanyId = vm.CompanyOptions[0].CompanyId;
        }

        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppWorkPlaceEditViewModel vm)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        NormalizeCheckboxes(vm);
        await ValidateWorkPlaceAsync(vm);

        if (!ModelState.IsValid)
        {
            vm.CompanyOptions = await LoadCompanyOptionsAsync();
            return View("Edit", vm);
        }

        var workPlace = MapToEntity(vm);
        workPlace.CreatedDate = DateTime.UtcNow;
        _context.AppWorkPlaces.Add(workPlace);
        await _context.SaveChangesAsync();

        if (workPlace.IsDefault == true)
        {
            await ClearOtherDefaultsAsync(workPlace.CompanyId, workPlace.RecId);
            await _context.SaveChangesAsync();
        }

        await _appLogService.WriteInfoAsync(
            "WorkPlace",
            "Created",
            $"Şube oluşturuldu. Code={workPlace.WorkPlaceCode}",
            newValue: await BuildAuditValueAsync(workPlace));

        TempData["SuccessMessage"] = "Şube kaydedildi.";
        return RedirectToAction(nameof(Index), new { companyId = workPlace.CompanyId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var workPlace = await _context.AppWorkPlaces.AsNoTracking().FirstOrDefaultAsync(x => x.RecId == id);
        if (workPlace is null)
        {
            return NotFound();
        }

        var vm = MapToViewModel(workPlace);
        vm.CompanyOptions = await LoadCompanyOptionsAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppWorkPlaceEditViewModel vm)
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
        var workPlace = await _context.AppWorkPlaces.FirstOrDefaultAsync(x => x.RecId == id);
        if (workPlace is null)
        {
            return NotFound();
        }

        var oldAudit = await BuildAuditValueAsync(workPlace);
        await ValidateWorkPlaceAsync(vm);

        if (!ModelState.IsValid)
        {
            vm.CompanyOptions = await LoadCompanyOptionsAsync();
            return View(vm);
        }

        ApplyViewModel(workPlace, vm);
        await _context.SaveChangesAsync();

        if (workPlace.IsDefault == true)
        {
            await ClearOtherDefaultsAsync(workPlace.CompanyId, workPlace.RecId);
            await _context.SaveChangesAsync();
        }

        await _appLogService.WriteInfoAsync(
            "WorkPlace",
            "Updated",
            $"Şube güncellendi. Code={workPlace.WorkPlaceCode}",
            oldValue: oldAudit,
            newValue: await BuildAuditValueAsync(workPlace));

        TempData["SuccessMessage"] = "Şube güncellendi.";
        return RedirectToAction(nameof(Index), new { companyId = workPlace.CompanyId });
    }

    private IActionResult? EnsureAdmin()
    {
        if (!string.Equals(User.FindFirst("IsRight")?.Value, "true", StringComparison.Ordinal))
        {
            return RedirectToAction("Index", "Home");
        }

        return null;
    }

    private void NormalizeCheckboxes(AppWorkPlaceEditViewModel vm)
    {
        vm.IsDefault = string.Equals(Request.Form["IsDefault"], "true", StringComparison.Ordinal);
        vm.IsActive = string.Equals(Request.Form["IsActive"], "true", StringComparison.Ordinal);
    }

    private async Task ValidateWorkPlaceAsync(AppWorkPlaceEditViewModel vm)
    {
        var companyExists = await _context.AppCompanies.AnyAsync(x => x.RecId == vm.CompanyId && x.IsActive == true);
        if (!companyExists)
        {
            ModelState.AddModelError(nameof(vm.CompanyId), "Geçerli bir şirket seçiniz.");
        }

        var codeExists = await _context.AppWorkPlaces.AnyAsync(x =>
            x.CompanyId == vm.CompanyId
            && x.WorkPlaceCode == vm.WorkPlaceCode.Trim()
            && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.WorkPlaceCode), "Bu şirket için aynı şube kodu zaten kullanılıyor.");
        }
    }

    private async Task<List<CompanyOptionViewModel>> LoadCompanyOptionsAsync()
    {
        return await _context.AppCompanies
            .AsNoTracking()
            .Where(x => x.IsActive == true)
            .OrderBy(x => x.CompanyCode)
            .Select(x => new CompanyOptionViewModel
            {
                CompanyId = x.RecId,
                DisplayName = (x.CompanyCode ?? string.Empty) + " - " + (x.CompanyName ?? string.Empty)
            })
            .ToListAsync();
    }

    private async Task ClearOtherDefaultsAsync(int companyId, int currentWorkPlaceId)
    {
        var others = await _context.AppWorkPlaces
            .Where(x => x.CompanyId == companyId && x.RecId != currentWorkPlaceId && x.IsDefault == true)
            .ToListAsync();

        foreach (var item in others)
        {
            item.IsDefault = false;
        }
    }

    private static AppWorkPlaceEditViewModel MapToViewModel(AppWorkPlace workPlace)
    {
        return new AppWorkPlaceEditViewModel
        {
            RecId = workPlace.RecId,
            CompanyId = workPlace.CompanyId,
            WorkPlaceCode = workPlace.WorkPlaceCode ?? string.Empty,
            WorkPlaceName = workPlace.WorkPlaceName ?? string.Empty,
            Address = workPlace.Address,
            City = workPlace.City,
            Phone = workPlace.Phone,
            IsDefault = workPlace.IsDefault == true,
            IsActive = workPlace.IsActive != false
        };
    }

    private static AppWorkPlace MapToEntity(AppWorkPlaceEditViewModel vm)
    {
        var workPlace = new AppWorkPlace();
        ApplyViewModel(workPlace, vm);
        return workPlace;
    }

    private static void ApplyViewModel(AppWorkPlace workPlace, AppWorkPlaceEditViewModel vm)
    {
        workPlace.CompanyId = vm.CompanyId;
        workPlace.WorkPlaceCode = vm.WorkPlaceCode.Trim();
        workPlace.WorkPlaceName = vm.WorkPlaceName.Trim();
        workPlace.Address = vm.Address?.Trim();
        workPlace.City = vm.City?.Trim();
        workPlace.Phone = vm.Phone?.Trim();
        workPlace.IsDefault = vm.IsDefault;
        workPlace.IsActive = vm.IsActive;
    }

    private async Task<string> BuildAuditValueAsync(AppWorkPlace workPlace)
    {
        var companyName = await _context.AppCompanies
            .AsNoTracking()
            .Where(x => x.RecId == workPlace.CompanyId)
            .Select(x => x.CompanyName)
            .FirstOrDefaultAsync();

        var sb = new StringBuilder();
        sb.Append($"Company={companyName ?? workPlace.CompanyId.ToString()}, Code={workPlace.WorkPlaceCode}");
        sb.Append($", Name={workPlace.WorkPlaceName}, IsDefault={workPlace.IsDefault == true}, IsActive={workPlace.IsActive == true}");
        return sb.ToString();
    }
}
