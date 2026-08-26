using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

public class AppVatWithholdingDefinitionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;
    private readonly IUserRightService _userRightService;

    public AppVatWithholdingDefinitionsController(ApplicationDbContext context, IAppLogService appLogService, IUserRightService userRightService)
    {
        _context = context;
        _appLogService = appLogService;
        _userRightService = userRightService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool showPassive = false)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var query = _context.AppVatWithholdingDefinitions.AsNoTracking();

        if (showPassive)
        {
            query = query.Where(x => x.IsActive == false);
        }
        else
        {
            query = query.Where(x => x.IsActive != false);
        }

        var items = await query
            .OrderBy(x => x.VatRate)
            .ThenBy(x => x.WithholdingCode)
            .Select(x => new AppVatWithholdingDefinitionListItemViewModel
            {
                RecId = x.RecId,
                WithholdingCode = x.WithholdingCode,
                WithholdingName = x.WithholdingName,
                VatRate = x.VatRate,
                WithholdingRate = x.WithholdingRate,
                AccountCode = x.AccountCode,
                IsActive = x.IsActive != false
            })
            .ToListAsync();

        return View(new AppVatWithholdingDefinitionListViewModel
        {
            ShowPassive = showPassive,
            Items = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var vm = new AppVatWithholdingDefinitionEditViewModel();
        await PopulateAccountCodeOptionsAsync(vm);
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppVatWithholdingDefinitionEditViewModel vm)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        NormalizeCheckboxes(vm);
        await ValidateVatWithholdingDefinitionAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateAccountCodeOptionsAsync(vm);
            return View("Edit", vm);
        }

        var entity = MapToEntity(vm);
        entity.CreatedDate = DateTime.UtcNow;
        _context.AppVatWithholdingDefinitions.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "VatWithholdingDefinition",
            "Created",
            $"Tevkifat KDV tanımı oluşturuldu. Code={entity.WithholdingCode}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Tevkifat KDV tanımı kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var entity = await _context.AppVatWithholdingDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var vm = MapToViewModel(entity);
        await PopulateAccountCodeOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppVatWithholdingDefinitionEditViewModel vm)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        if (id != vm.RecId)
        {
            return NotFound();
        }

        NormalizeCheckboxes(vm);
        var entity = await _context.AppVatWithholdingDefinitions.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        await ValidateVatWithholdingDefinitionAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateAccountCodeOptionsAsync(vm);
            return View(vm);
        }

        ApplyViewModel(entity, vm);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "VatWithholdingDefinition",
            "Updated",
            $"Tevkifat KDV tanımı güncellendi. Code={entity.WithholdingCode}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Tevkifat KDV tanımı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var entity = await _context.AppVatWithholdingDefinitions.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var withholdingCode = entity.WithholdingCode;
        var oldAudit = BuildAuditValue(entity);

        _context.AppVatWithholdingDefinitions.Remove(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "VatWithholdingDefinition",
            "Deleted",
            $"Tevkifat KDV tanımı silindi. Code={withholdingCode}",
            oldValue: oldAudit);

        TempData["SuccessMessage"] = "Tevkifat KDV tanımı silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult?> EnsureAdmin()
    {
        var action = AppScreenRights.ResolveAction(ControllerContext.ActionDescriptor.ActionName);
        var allowed = await _userRightService.HasScreenAccessAsync(User, ControllerContext.ActionDescriptor.ControllerName, action);
        return allowed ? null : RedirectToAction("Index", "Home");
    }

    private void NormalizeCheckboxes(AppVatWithholdingDefinitionEditViewModel vm)
    {
        vm.IsActive = string.Equals(Request.Form["IsActive"], "true", StringComparison.Ordinal);
    }

    private async Task ValidateVatWithholdingDefinitionAsync(AppVatWithholdingDefinitionEditViewModel vm)
    {
        var code = vm.WithholdingCode.Trim();
        var codeExists = await _context.AppVatWithholdingDefinitions.AnyAsync(x =>
            x.WithholdingCode == code && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.WithholdingCode), "Bu tevkifat kodu zaten kullanılıyor.");
        }

        await ValidateAccountCodeAsync(vm.AccountCode, nameof(vm.AccountCode), "Tevkifatın gideceği");
    }

    private async Task ValidateAccountCodeAsync(string? accountCode, string fieldName, string label)
    {
        if (string.IsNullOrWhiteSpace(accountCode))
        {
            return;
        }

        var code = accountCode.Trim();
        var exists = await _context.AppAccountPlans.AnyAsync(x => x.AccountCode == code && x.IsActive != false);
        if (!exists)
        {
            ModelState.AddModelError(fieldName, $"{label} muhasebe kodu hesap planında bulunamadı: {code}");
        }
    }

    private async Task PopulateAccountCodeOptionsAsync(AppVatWithholdingDefinitionEditViewModel vm)
    {
        var accounts = await _context.AppAccountPlans
            .AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.AccountCode)
            .Select(x => new { x.AccountCode, x.AccountName })
            .ToListAsync();

        var selectedCode = vm.AccountCode ?? string.Empty;

        vm.AccountCodeOptions = accounts
            .Select(x => new SelectListItem
            {
                Value = x.AccountCode,
                Text = $"{x.AccountCode} - {x.AccountName}",
                Selected = string.Equals(x.AccountCode, selectedCode, StringComparison.OrdinalIgnoreCase)
            })
            .ToList();
    }

    private static AppVatWithholdingDefinitionEditViewModel MapToViewModel(AppVatWithholdingDefinition entity)
    {
        return new AppVatWithholdingDefinitionEditViewModel
        {
            RecId = entity.RecId,
            WithholdingCode = entity.WithholdingCode,
            WithholdingName = entity.WithholdingName,
            VatRate = entity.VatRate,
            WithholdingRate = entity.WithholdingRate,
            AccountCode = entity.AccountCode,
            IsActive = entity.IsActive != false
        };
    }

    private static AppVatWithholdingDefinition MapToEntity(AppVatWithholdingDefinitionEditViewModel vm)
    {
        var entity = new AppVatWithholdingDefinition();
        ApplyViewModel(entity, vm);
        return entity;
    }

    private static void ApplyViewModel(AppVatWithholdingDefinition entity, AppVatWithholdingDefinitionEditViewModel vm)
    {
        entity.WithholdingCode = vm.WithholdingCode.Trim();
        entity.WithholdingName = vm.WithholdingName.Trim();
        entity.VatRate = vm.VatRate;
        entity.WithholdingRate = vm.WithholdingRate;
        entity.AccountCode = NormalizeOptionalCode(vm.AccountCode);
        entity.IsActive = vm.IsActive;
    }

    private static string? NormalizeOptionalCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string BuildAuditValue(AppVatWithholdingDefinition entity)
    {
        var sb = new StringBuilder();
        sb.Append($"Code={entity.WithholdingCode}, Name={entity.WithholdingName}, VatRate={entity.VatRate}, WithholdingRate={entity.WithholdingRate}");
        sb.Append($", Account={entity.AccountCode ?? "-"}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
