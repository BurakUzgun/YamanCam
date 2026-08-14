using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

public class AppVatDefinitionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;
    private readonly IUserRightService _userRightService;

    public AppVatDefinitionsController(ApplicationDbContext context, IAppLogService appLogService, IUserRightService userRightService)
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

        var query = _context.AppVatDefinitions.AsNoTracking();

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
            .ThenBy(x => x.VatCode)
            .Select(x => new AppVatDefinitionListItemViewModel
            {
                RecId = x.RecId,
                VatCode = x.VatCode,
                VatName = x.VatName,
                VatRate = x.VatRate,
                PurchaseAccountCode = x.PurchaseAccountCode,
                SalesAccountCode = x.SalesAccountCode,
                PurchaseReturnAccountCode = x.PurchaseReturnAccountCode,
                SalesReturnAccountCode = x.SalesReturnAccountCode,
                IsActive = x.IsActive != false
            })
            .ToListAsync();

        return View(new AppVatDefinitionListViewModel
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

        var vm = new AppVatDefinitionEditViewModel();
        await PopulateAccountCodeOptionsAsync(vm);
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppVatDefinitionEditViewModel vm)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        NormalizeCheckboxes(vm);
        await ValidateVatDefinitionAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateAccountCodeOptionsAsync(vm);
            return View("Edit", vm);
        }

        var entity = MapToEntity(vm);
        entity.CreatedDate = DateTime.UtcNow;
        _context.AppVatDefinitions.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "VatDefinition",
            "Created",
            $"KDV tanımı oluşturuldu. Code={entity.VatCode}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "KDV tanımı kaydedildi.";
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

        var entity = await _context.AppVatDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.RecId == id);
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
    public async Task<IActionResult> Edit(int id, AppVatDefinitionEditViewModel vm)
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
        var entity = await _context.AppVatDefinitions.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        await ValidateVatDefinitionAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateAccountCodeOptionsAsync(vm);
            return View(vm);
        }

        ApplyViewModel(entity, vm);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "VatDefinition",
            "Updated",
            $"KDV tanımı güncellendi. Code={entity.VatCode}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "KDV tanımı güncellendi.";
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

        var entity = await _context.AppVatDefinitions.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var vatCode = entity.VatCode;
        var oldAudit = BuildAuditValue(entity);

        _context.AppVatDefinitions.Remove(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "VatDefinition",
            "Deleted",
            $"KDV tanımı silindi. Code={vatCode}",
            oldValue: oldAudit);

        TempData["SuccessMessage"] = "KDV tanımı silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult?> EnsureAdmin()
    {
        var action = AppScreenRights.ResolveAction(ControllerContext.ActionDescriptor.ActionName);
        var allowed = await _userRightService.HasScreenAccessAsync(User, ControllerContext.ActionDescriptor.ControllerName, action);
        return allowed ? null : RedirectToAction("Index", "Home");
    }

    private void NormalizeCheckboxes(AppVatDefinitionEditViewModel vm)
    {
        vm.IsActive = string.Equals(Request.Form["IsActive"], "true", StringComparison.Ordinal);
    }

    private async Task ValidateVatDefinitionAsync(AppVatDefinitionEditViewModel vm)
    {
        var code = vm.VatCode.Trim();
        var codeExists = await _context.AppVatDefinitions.AnyAsync(x =>
            x.VatCode == code && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.VatCode), "Bu KDV kodu zaten kullanılıyor.");
        }

        await ValidateAccountCodeAsync(vm.PurchaseAccountCode, nameof(vm.PurchaseAccountCode), "Alış");
        await ValidateAccountCodeAsync(vm.SalesAccountCode, nameof(vm.SalesAccountCode), "Satış");
        await ValidateAccountCodeAsync(vm.PurchaseReturnAccountCode, nameof(vm.PurchaseReturnAccountCode), "Alış iade");
        await ValidateAccountCodeAsync(vm.SalesReturnAccountCode, nameof(vm.SalesReturnAccountCode), "Satış iade");
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

    private async Task PopulateAccountCodeOptionsAsync(AppVatDefinitionEditViewModel vm)
    {
        var accounts = await _context.AppAccountPlans
            .AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.AccountCode)
            .Select(x => new { x.AccountCode, x.AccountName })
            .ToListAsync();

        var selectedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            vm.PurchaseAccountCode ?? string.Empty,
            vm.SalesAccountCode ?? string.Empty,
            vm.PurchaseReturnAccountCode ?? string.Empty,
            vm.SalesReturnAccountCode ?? string.Empty
        };

        vm.AccountCodeOptions = accounts
            .Select(x => new SelectListItem
            {
                Value = x.AccountCode,
                Text = $"{x.AccountCode} - {x.AccountName}",
                Selected = selectedCodes.Contains(x.AccountCode)
            })
            .ToList();
    }

    private static AppVatDefinitionEditViewModel MapToViewModel(AppVatDefinition entity)
    {
        return new AppVatDefinitionEditViewModel
        {
            RecId = entity.RecId,
            VatCode = entity.VatCode,
            VatName = entity.VatName,
            VatRate = entity.VatRate,
            PurchaseAccountCode = entity.PurchaseAccountCode,
            SalesAccountCode = entity.SalesAccountCode,
            PurchaseReturnAccountCode = entity.PurchaseReturnAccountCode,
            SalesReturnAccountCode = entity.SalesReturnAccountCode,
            IsActive = entity.IsActive != false
        };
    }

    private static AppVatDefinition MapToEntity(AppVatDefinitionEditViewModel vm)
    {
        var entity = new AppVatDefinition();
        ApplyViewModel(entity, vm);
        return entity;
    }

    private static void ApplyViewModel(AppVatDefinition entity, AppVatDefinitionEditViewModel vm)
    {
        entity.VatCode = vm.VatCode.Trim();
        entity.VatName = vm.VatName.Trim();
        entity.VatRate = vm.VatRate;
        entity.PurchaseAccountCode = NormalizeOptionalCode(vm.PurchaseAccountCode);
        entity.SalesAccountCode = NormalizeOptionalCode(vm.SalesAccountCode);
        entity.PurchaseReturnAccountCode = NormalizeOptionalCode(vm.PurchaseReturnAccountCode);
        entity.SalesReturnAccountCode = NormalizeOptionalCode(vm.SalesReturnAccountCode);
        entity.IsActive = vm.IsActive;
    }

    private static string? NormalizeOptionalCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string BuildAuditValue(AppVatDefinition entity)
    {
        var sb = new StringBuilder();
        sb.Append($"Code={entity.VatCode}, Name={entity.VatName}, Rate={entity.VatRate}");
        sb.Append($", Purchase={entity.PurchaseAccountCode ?? "-"}, Sales={entity.SalesAccountCode ?? "-"}");
        sb.Append($", PurchaseReturn={entity.PurchaseReturnAccountCode ?? "-"}, SalesReturn={entity.SalesReturnAccountCode ?? "-"}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
