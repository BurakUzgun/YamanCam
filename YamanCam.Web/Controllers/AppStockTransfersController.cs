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
/// Şubeler Arası Transfer Fişi giriş ekranı. Çıkan şubeden giren şubeye stok aktarımı yapılır.
/// Satırlar App_Stock (malzeme kartları) referans alır, her satırda seçilen malzemenin birimi
/// otomatik önerilir. Fiş üzerinde döviz/KDV yoktur; yalnızca miktar x birim fiyat = toplam fiyat.
/// </summary>
public class AppStockTransfersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;
    private readonly IUserRightService _userRightService;

    public AppStockTransfersController(ApplicationDbContext context, IAppLogService appLogService, IUserRightService userRightService)
    {
        _context = context;
        _appLogService = appLogService;
        _userRightService = userRightService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool showDeleted = false)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var query = _context.AppStockTransfers.AsNoTracking();

        query = showDeleted
            ? query.Where(x => x.IsActive == false)
            : query.Where(x => x.IsActive != false);

        var items = await query
            .OrderByDescending(x => x.VoucherDate)
            .ThenByDescending(x => x.RecId)
            .Select(x => new AppStockTransferListItemViewModel
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

        return View(new AppStockTransferListViewModel
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

        var vm = new AppStockTransferEditViewModel();
        await PopulateSelectListsAsync(vm);
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppStockTransferEditViewModel vm)
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
            return View("Edit", vm);
        }

        var entity = new AppStockTransfer { IsActive = true };
        ApplyViewModel(entity, vm);
        entity.CreatedDate = DateTime.UtcNow;
        ApplyLines(entity, vm);

        _context.AppStockTransfers.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "StockTransfer",
            "Created",
            $"Şubeler Arası Transfer Fişi oluşturuldu. No={entity.VoucherNo}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Şubeler Arası Transfer Fişi kaydedildi.";
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

        var entity = await _context.AppStockTransfers
            .AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.RecId == id);

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
    public async Task<IActionResult> Edit(int id, AppStockTransferEditViewModel vm)
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

        var entity = await _context.AppStockTransfers
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
            return View(vm);
        }

        ApplyViewModel(entity, vm);
        _context.AppStockTransferLines.RemoveRange(entity.Lines);
        entity.Lines.Clear();
        ApplyLines(entity, vm);

        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "StockTransfer",
            "Updated",
            $"Şubeler Arası Transfer Fişi güncellendi. No={entity.VoucherNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Şubeler Arası Transfer Fişi güncellendi.";
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

        var entity = await _context.AppStockTransfers
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
            "StockTransfer",
            "Deleted",
            $"Şubeler Arası Transfer Fişi silindi. No={entity.VoucherNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Şubeler Arası Transfer Fişi silindi.";
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

        var entity = await _context.AppStockTransfers
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
            "StockTransfer",
            "Restored",
            $"Şubeler Arası Transfer Fişi geri yüklendi. No={entity.VoucherNo}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Şubeler Arası Transfer Fişi geri yüklendi.";
        return RedirectToAction(nameof(Index), new { showDeleted = true });
    }

    private async Task<IActionResult?> EnsureAdmin()
    {
        var action = AppScreenRights.ResolveAction(ControllerContext.ActionDescriptor.ActionName);
        var allowed = await _userRightService.HasScreenAccessAsync(User, ControllerContext.ActionDescriptor.ControllerName, action);
        return allowed ? null : RedirectToAction("Index", "Home");
    }

    private async Task PopulateSelectListsAsync(AppStockTransferEditViewModel vm)
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
            .Select(x => new { x.RecId, x.StockCode, x.StockName, x.StockUnitId })
            .ToListAsync();

        vm.StockOptions = stocks
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

    private async Task ValidateTransferAsync(AppStockTransferEditViewModel vm)
    {
        vm.VoucherNo = vm.VoucherNo.Trim().ToUpperInvariant();
        vm.TransactionType = AppStockTransferTransactionTypes.Transfer;
        ModelState.Remove(nameof(vm.TransactionType));

        var codeExists = await _context.AppStockTransfers.AnyAsync(x =>
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
            .Where(x => x.StockId > 0 || x.Quantity != 0 || x.UnitPrice != 0)
            .ToList();

        if (vm.Lines.Count < 1)
        {
            ModelState.AddModelError(string.Empty, "En az 1 stok satırı girilmelidir.");
        }

        for (var i = 0; i < vm.Lines.Count; i++)
        {
            var line = vm.Lines[i];
            line.LineNo = i + 1;

            if (line.StockId <= 0 || !await _context.AppStocks.AnyAsync(x => x.RecId == line.StockId))
            {
                ModelState.AddModelError($"Lines[{i}].StockId", "Geçerli bir stok seçilmelidir.");
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

            line.TotalPrice = Math.Round(line.Quantity * line.UnitPrice, 2, MidpointRounding.AwayFromZero);
        }

        vm.TotalAmount = vm.Lines.Sum(x => x.TotalPrice);
    }

    private AppStockTransferEditViewModel MapToViewModel(AppStockTransfer entity)
    {
        return new AppStockTransferEditViewModel
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
                .Select(x => new AppStockTransferLineEditViewModel
                {
                    RecId = x.RecId,
                    LineNo = x.LineNo,
                    StockId = x.StockId,
                    StockUnitId = x.StockUnitId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    TotalPrice = x.TotalPrice
                })
                .ToList()
        };
    }

    private static void ApplyViewModel(AppStockTransfer entity, AppStockTransferEditViewModel vm)
    {
        entity.VoucherNo = vm.VoucherNo.Trim().ToUpperInvariant();
        entity.VoucherDate = vm.VoucherDate;
        entity.InWorkPlaceId = vm.InWorkPlaceId;
        entity.OutWorkPlaceId = vm.OutWorkPlaceId;
        entity.TransactionType = AppStockTransferTransactionTypes.Transfer;
        entity.SpecialCode = string.IsNullOrWhiteSpace(vm.SpecialCode) ? null : vm.SpecialCode.Trim();
        entity.TotalAmount = vm.TotalAmount;
    }

    private static void ApplyLines(AppStockTransfer entity, AppStockTransferEditViewModel vm)
    {
        foreach (var line in vm.Lines)
        {
            entity.Lines.Add(new AppStockTransferLine
            {
                LineNo = line.LineNo,
                StockId = line.StockId,
                StockUnitId = line.StockUnitId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                TotalPrice = line.TotalPrice,
                CreatedDate = DateTime.UtcNow
            });
        }
    }

    private static string BuildAuditValue(AppStockTransfer entity)
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
