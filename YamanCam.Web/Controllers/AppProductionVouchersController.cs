using System.Globalization;
using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

/// <summary>
/// Stok Ürün Üretim Fişi giriş ekranı. Seçilen şubede hammaddeden mamül üretimi yapılır.
/// Her satırda tüketilen hammadde + fire oranı ve üretilen mamül miktarı girilir. Fiş
/// üzerinde döviz/KDV/tutar yoktur. Yapı AppStockOpeningsController ile aynıdır; satırda
/// iki stok alanı (hammadde + mamül) bulunur.
/// </summary>
public class AppProductionVouchersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;
    private readonly IUserRightService _userRightService;
    private readonly IAppSettingService _appSettingService;

    public AppProductionVouchersController(
        ApplicationDbContext context,
        IAppLogService appLogService,
        IUserRightService userRightService,
        IAppSettingService appSettingService)
    {
        _context = context;
        _appLogService = appLogService;
        _userRightService = userRightService;
        _appSettingService = appSettingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool showDeleted = false)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var query = _context.AppProductionVouchers.AsNoTracking();

        query = showDeleted
            ? query.Where(x => x.IsActive == false)
            : query.Where(x => x.IsActive != false);

        var items = await query
            .OrderByDescending(x => x.VoucherDate)
            .ThenByDescending(x => x.RecId)
            .Select(x => new AppProductionVoucherListItemViewModel
            {
                RecId = x.RecId,
                VoucherNo = x.VoucherNo,
                VoucherDate = x.VoucherDate,
                ProductionDate = x.ProductionDate,
                WorkPlaceName = x.WorkPlace != null ? x.WorkPlace.WorkPlaceName : null,
                TransactionType = x.TransactionType,
                SpecialCode = x.SpecialCode,
                LineCount = x.Lines.Count
            })
            .ToListAsync();

        ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();

        return View(new AppProductionVoucherListViewModel
        {
            ShowDeleted = showDeleted,
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

        var vm = new AppProductionVoucherEditViewModel();
        await PopulateSelectListsAsync(vm);
        ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppProductionVoucherEditViewModel vm)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        await ValidateVoucherAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();
            return View("Edit", vm);
        }

        var entity = new AppProductionVoucher { IsActive = true };
        ApplyViewModel(entity, vm);
        entity.CreatedDate = DateTime.UtcNow;
        ApplyLines(entity, vm);

        _context.AppProductionVouchers.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "ProductionVoucher",
            "Created",
            $"Stok Ürün Üretim Fişi oluşturuldu. No={entity.VoucherNo}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Stok Ürün Üretim Fişi kaydedildi.";
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

        var entity = await _context.AppProductionVouchers
            .AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.RecId == id);

        if (entity is null)
        {
            return NotFound();
        }

        var vm = MapToViewModel(entity);
        await PopulateSelectListsAsync(vm);
        ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppProductionVoucherEditViewModel vm)
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

        var entity = await _context.AppProductionVouchers
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.RecId == id);

        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        await ValidateVoucherAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();
            return View(vm);
        }

        ApplyViewModel(entity, vm);
        _context.AppProductionVoucherLines.RemoveRange(entity.Lines);
        entity.Lines.Clear();
        ApplyLines(entity, vm);

        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "ProductionVoucher",
            "Updated",
            $"Stok Ürün Üretim Fişi güncellendi. No={entity.VoucherNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Stok Ürün Üretim Fişi güncellendi.";
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

        var entity = await _context.AppProductionVouchers
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.RecId == id);

        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        entity.IsActive = false;
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "ProductionVoucher",
            "Deleted",
            $"Stok Ürün Üretim Fişi silindi. No={entity.VoucherNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Stok Ürün Üretim Fişi silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var entity = await _context.AppProductionVouchers
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.RecId == id);

        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        entity.IsActive = true;
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "ProductionVoucher",
            "Restored",
            $"Stok Ürün Üretim Fişi geri yüklendi. No={entity.VoucherNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Stok Ürün Üretim Fişi geri yüklendi.";
        return RedirectToAction(nameof(Index), new { showDeleted = true });
    }

    private async Task<IActionResult?> EnsureAdmin()
    {
        var action = AppScreenRights.ResolveAction(ControllerContext.ActionDescriptor.ActionName);
        var allowed = await _userRightService.HasScreenAccessAsync(User, ControllerContext.ActionDescriptor.ControllerName, action);
        return allowed ? null : RedirectToAction("Index", "Home");
    }

    private async Task PopulateSelectListsAsync(AppProductionVoucherEditViewModel vm)
    {
        var workPlaces = await _context.AppWorkPlaces.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.WorkPlaceCode)
            .Select(x => new { x.RecId, x.WorkPlaceCode, x.WorkPlaceName })
            .ToListAsync();

        vm.WorkPlaceOptions = workPlaces
            .Select(x => new SelectListItem($"{x.WorkPlaceCode} - {x.WorkPlaceName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        vm.TransactionTypeOptions = AppProductionVoucherTransactionTypes.All
            .Select(x => new SelectListItem(x, x))
            .ToList();

        var stocks = await _context.AppStocks.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.StockCode)
            .Select(x => new { x.RecId, x.StockCode, x.StockName })
            .ToListAsync();

        vm.StockOptions = stocks
            .Select(x => new SelectListItem($"{x.StockCode} - {x.StockName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        // Mamül -> hammadde eşlemesi (üretim tanımlarından). Satırda mamül seçilince
        // hammadde alanının otomatik dolması için kullanılır.
        var definitions = await _context.AppProductionDefinitions.AsNoTracking()
            .Where(x => x.IsActive != false)
            .Select(x => new { x.ProductStockId, x.RawMaterialStockId })
            .ToListAsync();

        ViewData["ProductRawMaterialMap"] = definitions
            .GroupBy(x => x.ProductStockId)
            .ToDictionary(
                g => g.Key.ToString(CultureInfo.InvariantCulture),
                g => g.First().RawMaterialStockId.ToString(CultureInfo.InvariantCulture));
    }

    private async Task ValidateVoucherAsync(AppProductionVoucherEditViewModel vm)
    {
        vm.VoucherNo = vm.VoucherNo.Trim().ToUpperInvariant();
        vm.TransactionType = string.IsNullOrWhiteSpace(vm.TransactionType)
            ? AppProductionVoucherTransactionTypes.Uretim
            : vm.TransactionType.Trim();

        ModelState.Remove(nameof(vm.TransactionType));
        if (!AppProductionVoucherTransactionTypes.All.Contains(vm.TransactionType))
        {
            ModelState.AddModelError(nameof(vm.TransactionType), "Geçersiz işlem türü.");
        }

        if (vm.ProductionDate == default)
        {
            vm.ProductionDate = vm.VoucherDate;
        }

        var codeExists = await _context.AppProductionVouchers.AnyAsync(x =>
            x.VoucherNo == vm.VoucherNo && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.VoucherNo), "Bu evrak no zaten kullanılıyor.");
        }

        if (!vm.WorkPlaceId.HasValue ||
            !await _context.AppWorkPlaces.AnyAsync(x => x.RecId == vm.WorkPlaceId.Value))
        {
            ModelState.AddModelError(nameof(vm.WorkPlaceId), "Geçerli bir şube seçilmelidir.");
        }

        vm.Lines = vm.Lines
            .Where(x => x.RawMaterialStockId > 0 || x.ProductStockId > 0 || x.RawMaterialQuantity != 0 || x.ProductQuantity != 0)
            .ToList();

        if (vm.Lines.Count < 1)
        {
            ModelState.AddModelError(string.Empty, "En az 1 üretim satırı girilmelidir.");
        }

        for (var i = 0; i < vm.Lines.Count; i++)
        {
            var line = vm.Lines[i];
            line.LineNo = i + 1;

            if (line.RawMaterialStockId <= 0 || !await _context.AppStocks.AnyAsync(x => x.RecId == line.RawMaterialStockId))
            {
                ModelState.AddModelError($"Lines[{i}].RawMaterialStockId", "Geçerli bir hammadde seçilmelidir.");
            }

            if (line.ProductStockId <= 0 || !await _context.AppStocks.AnyAsync(x => x.RecId == line.ProductStockId))
            {
                ModelState.AddModelError($"Lines[{i}].ProductStockId", "Geçerli bir mamül seçilmelidir.");
            }

            if (line.RawMaterialStockId > 0 && line.RawMaterialStockId == line.ProductStockId)
            {
                ModelState.AddModelError($"Lines[{i}].ProductStockId", "Hammadde ile mamül aynı stok olamaz.");
            }

            if (line.RawMaterialQuantity <= 0)
            {
                ModelState.AddModelError($"Lines[{i}].RawMaterialQuantity", "Hammadde miktarı sıfırdan büyük olmalıdır.");
            }

            if (line.ProductQuantity <= 0)
            {
                ModelState.AddModelError($"Lines[{i}].ProductQuantity", "Mamül miktarı sıfırdan büyük olmalıdır.");
            }

            if (line.WasteRate is < 0 or > 100)
            {
                ModelState.AddModelError($"Lines[{i}].WasteRate", "Fire oranı 0-100 arasında olmalıdır.");
            }
        }
    }

    private AppProductionVoucherEditViewModel MapToViewModel(AppProductionVoucher entity)
    {
        return new AppProductionVoucherEditViewModel
        {
            RecId = entity.RecId,
            VoucherNo = entity.VoucherNo,
            VoucherDate = entity.VoucherDate,
            ProductionDate = entity.ProductionDate,
            WorkPlaceId = entity.WorkPlaceId,
            TransactionType = entity.TransactionType,
            SpecialCode = entity.SpecialCode,
            Lines = entity.Lines
                .OrderBy(x => x.LineNo)
                .Select(x => new AppProductionVoucherLineEditViewModel
                {
                    RecId = x.RecId,
                    LineNo = x.LineNo,
                    RawMaterialStockId = x.RawMaterialStockId,
                    RawMaterialQuantity = x.RawMaterialQuantity,
                    WasteRate = x.WasteRate,
                    ProductQuantity = x.ProductQuantity,
                    ProductStockId = x.ProductStockId
                })
                .ToList()
        };
    }

    private static void ApplyViewModel(AppProductionVoucher entity, AppProductionVoucherEditViewModel vm)
    {
        entity.VoucherNo = vm.VoucherNo.Trim().ToUpperInvariant();
        entity.VoucherDate = vm.VoucherDate;
        entity.ProductionDate = vm.ProductionDate == default ? vm.VoucherDate : vm.ProductionDate;
        entity.WorkPlaceId = vm.WorkPlaceId;
        entity.TransactionType = vm.TransactionType;
        entity.SpecialCode = string.IsNullOrWhiteSpace(vm.SpecialCode) ? null : vm.SpecialCode.Trim();
    }

    private static void ApplyLines(AppProductionVoucher entity, AppProductionVoucherEditViewModel vm)
    {
        foreach (var line in vm.Lines)
        {
            entity.Lines.Add(new AppProductionVoucherLine
            {
                LineNo = line.LineNo,
                RawMaterialStockId = line.RawMaterialStockId,
                RawMaterialQuantity = line.RawMaterialQuantity,
                WasteRate = line.WasteRate,
                ProductQuantity = line.ProductQuantity,
                ProductStockId = line.ProductStockId,
                CreatedDate = DateTime.UtcNow
            });
        }
    }

    private static string BuildAuditValue(AppProductionVoucher entity)
    {
        var sb = new StringBuilder();
        sb.Append($"No={entity.VoucherNo}, Tarih={entity.VoucherDate:yyyy-MM-dd}, UretimTarih={entity.ProductionDate:yyyy-MM-dd}");
        sb.Append($", SubeId={entity.WorkPlaceId}, Tur={entity.TransactionType}");
        sb.Append($", SatirSayisi={entity.Lines.Count}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
