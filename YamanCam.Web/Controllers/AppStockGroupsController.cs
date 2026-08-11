using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

public class AppStockGroupsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;

    public AppStockGroupsController(ApplicationDbContext context, IAppLogService appLogService)
    {
        _context = context;
        _appLogService = appLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool showPassive = false)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var query = _context.AppStockGroups.AsNoTracking();

        if (showPassive)
        {
            query = query.Where(x => x.IsActive == false);
        }
        else
        {
            query = query.Where(x => x.IsActive != false);
        }

        var items = await query
            .OrderBy(x => x.GroupCode)
            .Select(x => new AppStockGroupListItemViewModel
            {
                RecId = x.RecId,
                GroupCode = x.GroupCode,
                GroupName = x.GroupName,
                PurchaseAccountCode = x.PurchaseAccountCode,
                SalesAccountCode = x.SalesAccountCode,
                IsActive = x.IsActive != false
            })
            .ToListAsync();

        return View(new AppStockGroupListViewModel
        {
            ShowPassive = showPassive,
            Items = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var vm = new AppStockGroupEditViewModel();
        await PopulateAccountCodeOptionsAsync(vm);
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppStockGroupEditViewModel vm)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        NormalizeCheckboxes(vm);
        await ValidateStockGroupAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateAccountCodeOptionsAsync(vm);
            return View("Edit", vm);
        }

        var entity = MapToEntity(vm);
        entity.CreatedDate = DateTime.UtcNow;
        _context.AppStockGroups.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "StockGroup",
            "Created",
            $"Stok grubu oluşturuldu. Code={entity.GroupCode}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Stok grubu kaydedildi.";
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

        var entity = await _context.AppStockGroups.AsNoTracking().FirstOrDefaultAsync(x => x.RecId == id);
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
    public async Task<IActionResult> Edit(int id, AppStockGroupEditViewModel vm)
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
        var entity = await _context.AppStockGroups.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        await ValidateStockGroupAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateAccountCodeOptionsAsync(vm);
            return View(vm);
        }

        ApplyViewModel(entity, vm);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "StockGroup",
            "Updated",
            $"Stok grubu güncellendi. Code={entity.GroupCode}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Stok grubu güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var entity = await _context.AppStockGroups.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var groupCode = entity.GroupCode;
        var oldAudit = BuildAuditValue(entity);

        _context.AppStockGroups.Remove(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "StockGroup",
            "Deleted",
            $"Stok grubu silindi. Code={groupCode}",
            oldValue: oldAudit);

        TempData["SuccessMessage"] = "Stok grubu silindi.";
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

    private void NormalizeCheckboxes(AppStockGroupEditViewModel vm)
    {
        vm.IsActive = string.Equals(Request.Form["IsActive"], "true", StringComparison.Ordinal);
    }

    private async Task ValidateStockGroupAsync(AppStockGroupEditViewModel vm)
    {
        var code = vm.GroupCode.Trim();
        var codeExists = await _context.AppStockGroups.AnyAsync(x =>
            x.GroupCode == code && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.GroupCode), "Bu stok grup kodu zaten kullanılıyor.");
        }

        await ValidateAccountCodeAsync(vm.PurchaseAccountCode, nameof(vm.PurchaseAccountCode), "Stok alış");
        await ValidateAccountCodeAsync(vm.SalesAccountCode, nameof(vm.SalesAccountCode), "Stok satış");
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

    private async Task PopulateAccountCodeOptionsAsync(AppStockGroupEditViewModel vm)
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
            vm.SalesAccountCode ?? string.Empty
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

    private static AppStockGroupEditViewModel MapToViewModel(AppStockGroup entity)
    {
        return new AppStockGroupEditViewModel
        {
            RecId = entity.RecId,
            GroupCode = entity.GroupCode,
            GroupName = entity.GroupName,
            PurchaseAccountCode = entity.PurchaseAccountCode,
            SalesAccountCode = entity.SalesAccountCode,
            IsActive = entity.IsActive != false
        };
    }

    private static AppStockGroup MapToEntity(AppStockGroupEditViewModel vm)
    {
        var entity = new AppStockGroup();
        ApplyViewModel(entity, vm);
        return entity;
    }

    private static void ApplyViewModel(AppStockGroup entity, AppStockGroupEditViewModel vm)
    {
        entity.GroupCode = vm.GroupCode.Trim();
        entity.GroupName = vm.GroupName.Trim();
        entity.PurchaseAccountCode = NormalizeOptionalCode(vm.PurchaseAccountCode);
        entity.SalesAccountCode = NormalizeOptionalCode(vm.SalesAccountCode);
        entity.IsActive = vm.IsActive;
    }

    private static string? NormalizeOptionalCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string BuildAuditValue(AppStockGroup entity)
    {
        var sb = new StringBuilder();
        sb.Append($"Code={entity.GroupCode}, Name={entity.GroupName}");
        sb.Append($", Purchase={entity.PurchaseAccountCode ?? "-"}, Sales={entity.SalesAccountCode ?? "-"}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
