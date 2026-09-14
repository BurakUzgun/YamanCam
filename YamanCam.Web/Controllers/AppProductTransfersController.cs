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
/// Şubeler Arası Transfer (Farklı Ürün) fişi giriş ekranı. Çıkan şubeden çıkan stok,
/// giren şubeye farklı bir ürün (aktarılan ürün) olarak girer. Her satırda çıkan stok
/// ile aktarılan (giren) stok ayrı seçilir; aktarılan ürün listesinde yalnızca türü
/// Mamül olan stoklar gösterilir. Yapı AppStockTransfersController ile aynıdır.
/// </summary>
public class AppProductTransfersController : Controller
{
    private static readonly string[] ProductStockTypes = { "Mamul", "Mamül" };

    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;
    private readonly IUserRightService _userRightService;
    private readonly IAppSettingService _appSettingService;

    public AppProductTransfersController(
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

        var query = _context.AppProductTransfers.AsNoTracking();

        query = showDeleted
            ? query.Where(x => x.IsActive == false)
            : query.Where(x => x.IsActive != false);

        var items = await query
            .OrderByDescending(x => x.VoucherDate)
            .ThenByDescending(x => x.RecId)
            .Select(x => new AppProductTransferListItemViewModel
            {
                RecId = x.RecId,
                VoucherNo = x.VoucherNo,
                VoucherDate = x.VoucherDate,
                InWorkPlaceName = x.InWorkPlace != null ? x.InWorkPlace.WorkPlaceName : null,
                OutWorkPlaceName = x.OutWorkPlace != null ? x.OutWorkPlace.WorkPlaceName : null,
                TransactionType = x.TransactionType,
                SpecialCode = x.SpecialCode,
                TotalAmount = x.TotalAmount
            })
            .ToListAsync();

        return View(new AppProductTransferListViewModel
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

        var vm = new AppProductTransferEditViewModel();
        await PopulateSelectListsAsync(vm);
        ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppProductTransferEditViewModel vm)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        await ValidateTransferAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();
            return View("Edit", vm);
        }

        var entity = new AppProductTransfer { IsActive = true };
        ApplyViewModel(entity, vm);
        entity.CreatedDate = DateTime.UtcNow;
        ApplyLines(entity, vm);

        _context.AppProductTransfers.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "ProductTransfer",
            "Created",
            $"Farklı Ürün Transfer fişi oluşturuldu. No={entity.VoucherNo}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Farklı Ürün Transfer fişi kaydedildi.";
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

        var entity = await _context.AppProductTransfers
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
    public async Task<IActionResult> Edit(int id, AppProductTransferEditViewModel vm)
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

        var entity = await _context.AppProductTransfers
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.RecId == id);

        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        await ValidateTransferAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            ViewData["Kusurat"] = await _appSettingService.GetKusuratMapAsync();
            return View(vm);
        }

        ApplyViewModel(entity, vm);
        _context.AppProductTransferLines.RemoveRange(entity.Lines);
        entity.Lines.Clear();
        ApplyLines(entity, vm);

        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "ProductTransfer",
            "Updated",
            $"Farklı Ürün Transfer fişi güncellendi. No={entity.VoucherNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Farklı Ürün Transfer fişi güncellendi.";
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

        var entity = await _context.AppProductTransfers
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
            "ProductTransfer",
            "Deleted",
            $"Farklı Ürün Transfer fişi silindi. No={entity.VoucherNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Farklı Ürün Transfer fişi silindi.";
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

        var entity = await _context.AppProductTransfers
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
            "ProductTransfer",
            "Restored",
            $"Farklı Ürün Transfer fişi geri yüklendi. No={entity.VoucherNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Farklı Ürün Transfer fişi geri yüklendi.";
        return RedirectToAction(nameof(Index), new { showDeleted = true });
    }

    private async Task<IActionResult?> EnsureAdmin()
    {
        var action = AppScreenRights.ResolveAction(ControllerContext.ActionDescriptor.ActionName);
        var allowed = await _userRightService.HasScreenAccessAsync(User, ControllerContext.ActionDescriptor.ControllerName, action);
        return allowed ? null : RedirectToAction("Index", "Home");
    }

    private async Task PopulateSelectListsAsync(AppProductTransferEditViewModel vm)
    {
        var workPlaces = await _context.AppWorkPlaces.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.WorkPlaceCode)
            .Select(x => new { x.RecId, x.WorkPlaceCode, x.WorkPlaceName })
            .ToListAsync();

        vm.WorkPlaceOptions = workPlaces
            .Select(x => new SelectListItem($"{x.WorkPlaceCode} - {x.WorkPlaceName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var stocks = await _context.AppStocks.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.StockCode)
            .Select(x => new { x.RecId, x.StockCode, x.StockName, x.StockUnitId, x.StockType })
            .ToListAsync();

        vm.StockOptions = stocks
            .Select(x => new SelectListItem($"{x.StockCode} - {x.StockName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        // Giren / Aktarılan ürün listesinde yalnızca türü Mamül olan stoklar.
        vm.ProductStockOptions = stocks
            .Where(x => x.StockType != null && ProductStockTypes.Contains(x.StockType.Trim(), StringComparer.OrdinalIgnoreCase))
            .Select(x => new SelectListItem($"{x.StockCode} - {x.StockName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        ViewData["StockUnits"] = stocks.ToDictionary(
            x => x.RecId.ToString(CultureInfo.InvariantCulture),
            x => x.StockUnitId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);

        var stockUnits = await _context.AppStockUnits.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.UnitCode)
            .Select(x => new { x.RecId, x.UnitCode, x.UnitName })
            .ToListAsync();

        vm.StockUnitOptions = stockUnits
            .Select(x => new SelectListItem($"{x.UnitCode} - {x.UnitName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();
    }

    private async Task ValidateTransferAsync(AppProductTransferEditViewModel vm)
    {
        vm.VoucherNo = vm.VoucherNo.Trim().ToUpperInvariant();
        vm.TransactionType = AppProductTransferTransactionTypes.DifferentProduct;
        ModelState.Remove(nameof(vm.TransactionType));

        var codeExists = await _context.AppProductTransfers.AnyAsync(x =>
            x.VoucherNo == vm.VoucherNo && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.VoucherNo), "Bu evrak no zaten kullanılıyor.");
        }

        if (!vm.InWorkPlaceId.HasValue ||
            !await _context.AppWorkPlaces.AnyAsync(x => x.RecId == vm.InWorkPlaceId.Value))
        {
            ModelState.AddModelError(nameof(vm.InWorkPlaceId), "Geçerli bir giren şube seçilmelidir.");
        }

        if (!vm.OutWorkPlaceId.HasValue ||
            !await _context.AppWorkPlaces.AnyAsync(x => x.RecId == vm.OutWorkPlaceId.Value))
        {
            ModelState.AddModelError(nameof(vm.OutWorkPlaceId), "Geçerli bir çıkan şube seçilmelidir.");
        }

        if (vm.InWorkPlaceId.HasValue && vm.OutWorkPlaceId.HasValue &&
            vm.InWorkPlaceId.Value == vm.OutWorkPlaceId.Value)
        {
            ModelState.AddModelError(nameof(vm.OutWorkPlaceId), "Giren şube ile çıkan şube aynı olamaz.");
        }

        vm.Lines = vm.Lines
            .Where(x => x.OutStockId > 0 || x.InStockId > 0 || x.Quantity != 0 || x.UnitPrice != 0)
            .ToList();

        if (vm.Lines.Count < 1)
        {
            ModelState.AddModelError(string.Empty, "En az 1 stok satırı girilmelidir.");
        }

        for (var i = 0; i < vm.Lines.Count; i++)
        {
            var line = vm.Lines[i];
            line.LineNo = i + 1;

            if (line.OutStockId <= 0 || !await _context.AppStocks.AnyAsync(x => x.RecId == line.OutStockId))
            {
                ModelState.AddModelError($"Lines[{i}].OutStockId", "Geçerli bir çıkan stok seçilmelidir.");
            }

            if (line.InStockId <= 0 || !await _context.AppStocks.AnyAsync(x => x.RecId == line.InStockId))
            {
                ModelState.AddModelError($"Lines[{i}].InStockId", "Geçerli bir giren (aktarılan) stok seçilmelidir.");
            }

            if (line.OutStockId > 0 && line.OutStockId == line.InStockId)
            {
                ModelState.AddModelError($"Lines[{i}].InStockId", "Çıkan stok ile giren stok aynı olamaz.");
            }

            if (line.StockUnitId.HasValue &&
                !await _context.AppStockUnits.AnyAsync(x => x.RecId == line.StockUnitId.Value))
            {
                ModelState.AddModelError($"Lines[{i}].StockUnitId", "Geçersiz stok birimi seçildi.");
            }

            if (line.Quantity <= 0)
            {
                ModelState.AddModelError($"Lines[{i}].Quantity", "Adet sıfırdan büyük olmalıdır.");
            }

            if (line.UnitPrice < 0)
            {
                ModelState.AddModelError($"Lines[{i}].UnitPrice", "Birim fiyat negatif olamaz.");
            }

            line.TotalPrice = Math.Round(line.Quantity * line.UnitPrice, 10, MidpointRounding.AwayFromZero);
        }

        vm.TotalAmount = vm.Lines.Sum(x => x.TotalPrice);
    }

    private AppProductTransferEditViewModel MapToViewModel(AppProductTransfer entity)
    {
        return new AppProductTransferEditViewModel
        {
            RecId = entity.RecId,
            VoucherNo = entity.VoucherNo,
            VoucherDate = entity.VoucherDate,
            InWorkPlaceId = entity.InWorkPlaceId,
            OutWorkPlaceId = entity.OutWorkPlaceId,
            TransactionType = entity.TransactionType,
            SpecialCode = entity.SpecialCode,
            TotalAmount = entity.TotalAmount,
            Lines = entity.Lines
                .OrderBy(x => x.LineNo)
                .Select(x => new AppProductTransferLineEditViewModel
                {
                    RecId = x.RecId,
                    LineNo = x.LineNo,
                    OutStockId = x.OutStockId,
                    InStockId = x.InStockId,
                    StockUnitId = x.StockUnitId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    TotalPrice = x.TotalPrice
                })
                .ToList()
        };
    }

    private static void ApplyViewModel(AppProductTransfer entity, AppProductTransferEditViewModel vm)
    {
        entity.VoucherNo = vm.VoucherNo.Trim().ToUpperInvariant();
        entity.VoucherDate = vm.VoucherDate;
        entity.InWorkPlaceId = vm.InWorkPlaceId;
        entity.OutWorkPlaceId = vm.OutWorkPlaceId;
        entity.TransactionType = AppProductTransferTransactionTypes.DifferentProduct;
        entity.SpecialCode = string.IsNullOrWhiteSpace(vm.SpecialCode) ? null : vm.SpecialCode.Trim();
        entity.TotalAmount = vm.TotalAmount;
    }

    private static void ApplyLines(AppProductTransfer entity, AppProductTransferEditViewModel vm)
    {
        foreach (var line in vm.Lines)
        {
            entity.Lines.Add(new AppProductTransferLine
            {
                LineNo = line.LineNo,
                OutStockId = line.OutStockId,
                InStockId = line.InStockId,
                StockUnitId = line.StockUnitId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                TotalPrice = line.TotalPrice,
                CreatedDate = DateTime.UtcNow
            });
        }
    }

    private static string BuildAuditValue(AppProductTransfer entity)
    {
        var sb = new StringBuilder();
        sb.Append($"No={entity.VoucherNo}, Tarih={entity.VoucherDate:yyyy-MM-dd}");
        sb.Append($", GirenSubeId={entity.InWorkPlaceId}, CikanSubeId={entity.OutWorkPlaceId}, Tur={entity.TransactionType}");
        sb.Append($", Toplam={entity.TotalAmount}");
        sb.Append($", SatirSayisi={entity.Lines.Count}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
