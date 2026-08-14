using System.Globalization;
using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

public class AppStocksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;
    private readonly IUserRightService _userRightService;

    public AppStocksController(ApplicationDbContext context, IAppLogService appLogService, IUserRightService userRightService)
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

        var query = _context.AppStocks.AsNoTracking();

        if (showPassive)
        {
            query = query.Where(x => x.IsActive == false);
        }
        else
        {
            query = query.Where(x => x.IsActive != false);
        }

        var items = await query
            .OrderBy(x => x.StockCode)
            .Select(x => new AppStockListItemViewModel
            {
                RecId = x.RecId,
                StockCode = x.StockCode,
                StockName = x.StockName,
                StockGroupName = x.StockGroup != null ? x.StockGroup.GroupName : null,
                StockUnitCode = x.StockUnit != null ? x.StockUnit.UnitCode : null,
                PurchaseVatLabel = x.PurchaseVat != null ? x.PurchaseVat.VatCode : null,
                SalesVatLabel = x.SalesVat != null ? x.SalesVat.VatCode : null,
                Barcode = x.Barcode,
                StockType = x.StockType,
                ProductionWeight = x.ProductionWeight,
                IsActive = x.IsActive != false
            })
            .ToListAsync();

        return View(new AppStockListViewModel
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

        var vm = new AppStockEditViewModel();
        await PopulateSelectListsAsync(vm);
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppStockEditViewModel vm)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        NormalizeCheckboxes(vm);
        await ValidateStockAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            return View("Edit", vm);
        }

        var entity = MapToEntity(vm);
        entity.CreatedDate = DateTime.UtcNow;
        _context.AppStocks.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "Stock",
            "Created",
            $"Stok oluşturuldu. Code={entity.StockCode}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Stok kaydedildi.";
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

        var entity = await _context.AppStocks.AsNoTracking().FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var vm = MapToViewModel(entity);
        await PopulateSelectListsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppStockEditViewModel vm)
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
        var entity = await _context.AppStocks.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        await ValidateStockAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            return View(vm);
        }

        ApplyViewModel(entity, vm);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "Stock",
            "Updated",
            $"Stok güncellendi. Code={entity.StockCode}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Stok güncellendi.";
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

        var entity = await _context.AppStocks.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var stockCode = entity.StockCode;
        var oldAudit = BuildAuditValue(entity);

        _context.AppStocks.Remove(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "Stock",
            "Deleted",
            $"Stok silindi. Code={stockCode}",
            oldValue: oldAudit);

        TempData["SuccessMessage"] = "Stok silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult?> EnsureAdmin()
    {
        var action = AppScreenRights.ResolveAction(ControllerContext.ActionDescriptor.ActionName);
        var allowed = await _userRightService.HasScreenAccessAsync(User, ControllerContext.ActionDescriptor.ControllerName, action);
        return allowed ? null : RedirectToAction("Index", "Home");
    }

    private void NormalizeCheckboxes(AppStockEditViewModel vm)
    {
        vm.IsActive = string.Equals(Request.Form["IsActive"], "true", StringComparison.Ordinal);
    }

    private async Task PopulateSelectListsAsync(AppStockEditViewModel vm)
    {
        var groups = await _context.AppStockGroups.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.GroupCode)
            .Select(x => new { x.RecId, x.GroupCode, x.GroupName })
            .ToListAsync();

        vm.StockGroupOptions = groups
            .Select(x => new SelectListItem($"{x.GroupCode} - {x.GroupName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var units = await _context.AppStockUnits.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.UnitCode)
            .Select(x => new { x.RecId, x.UnitCode, x.UnitName })
            .ToListAsync();

        vm.StockUnitOptions = units
            .Select(x => new SelectListItem($"{x.UnitCode} - {x.UnitName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var vats = await _context.AppVatDefinitions.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.VatCode)
            .Select(x => new { x.RecId, x.VatCode, x.VatName, x.VatRate })
            .ToListAsync();

        vm.VatOptions = vats
            .Select(x => new SelectListItem(
                $"{x.VatCode} - {x.VatName} (%{x.VatRate.ToString("0.##", CultureInfo.InvariantCulture)})",
                x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();
    }

    private async Task ValidateStockAsync(AppStockEditViewModel vm)
    {
        var code = vm.StockCode.Trim().ToUpperInvariant();
        vm.StockCode = code;

        var codeExists = await _context.AppStocks.AnyAsync(x =>
            x.StockCode == code && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.StockCode), "Bu stok kodu zaten kullanılıyor.");
        }

        if (vm.StockGroupId.HasValue &&
            !await _context.AppStockGroups.AnyAsync(x => x.RecId == vm.StockGroupId.Value))
        {
            ModelState.AddModelError(nameof(vm.StockGroupId), "Geçersiz stok grubu seçildi.");
        }

        if (vm.StockUnitId.HasValue &&
            !await _context.AppStockUnits.AnyAsync(x => x.RecId == vm.StockUnitId.Value))
        {
            ModelState.AddModelError(nameof(vm.StockUnitId), "Geçersiz stok birimi seçildi.");
        }

        if (vm.PurchaseVatId.HasValue &&
            !await _context.AppVatDefinitions.AnyAsync(x => x.RecId == vm.PurchaseVatId.Value))
        {
            ModelState.AddModelError(nameof(vm.PurchaseVatId), "Geçersiz alış KDV seçildi.");
        }

        if (vm.SalesVatId.HasValue &&
            !await _context.AppVatDefinitions.AnyAsync(x => x.RecId == vm.SalesVatId.Value))
        {
            ModelState.AddModelError(nameof(vm.SalesVatId), "Geçersiz satış KDV seçildi.");
        }

        if (vm.ProductionWeight.HasValue && vm.ProductionWeight.Value < 0)
        {
            ModelState.AddModelError(nameof(vm.ProductionWeight), "Üretim ağırlığı negatif olamaz.");
        }
    }

    private static AppStockEditViewModel MapToViewModel(AppStock entity)
    {
        return new AppStockEditViewModel
        {
            RecId = entity.RecId,
            StockCode = entity.StockCode,
            StockName = entity.StockName,
            StockGroupId = entity.StockGroupId,
            StockUnitId = entity.StockUnitId,
            PurchaseVatId = entity.PurchaseVatId,
            SalesVatId = entity.SalesVatId,
            Barcode = entity.Barcode,
            StockType = entity.StockType,
            ProductionWeight = entity.ProductionWeight,
            IsActive = entity.IsActive != false
        };
    }

    private static AppStock MapToEntity(AppStockEditViewModel vm)
    {
        var entity = new AppStock();
        ApplyViewModel(entity, vm);
        return entity;
    }

    private static void ApplyViewModel(AppStock entity, AppStockEditViewModel vm)
    {
        entity.StockCode = vm.StockCode.Trim().ToUpperInvariant();
        entity.StockName = vm.StockName.Trim();
        entity.StockGroupId = vm.StockGroupId;
        entity.StockUnitId = vm.StockUnitId;
        entity.PurchaseVatId = vm.PurchaseVatId;
        entity.SalesVatId = vm.SalesVatId;
        entity.Barcode = string.IsNullOrWhiteSpace(vm.Barcode) ? null : vm.Barcode.Trim();
        entity.StockType = string.IsNullOrWhiteSpace(vm.StockType) ? null : vm.StockType.Trim();
        entity.ProductionWeight = vm.ProductionWeight;
        entity.IsActive = vm.IsActive;
    }

    private static string BuildAuditValue(AppStock entity)
    {
        var sb = new StringBuilder();
        sb.Append($"Code={entity.StockCode}, Name={entity.StockName}");
        sb.Append($", GroupId={entity.StockGroupId}, UnitId={entity.StockUnitId}");
        sb.Append($", PurchaseVatId={entity.PurchaseVatId}, SalesVatId={entity.SalesVatId}");
        sb.Append($", Barcode={entity.Barcode}, Type={entity.StockType}");
        sb.Append($", ProductionWeight={entity.ProductionWeight}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
