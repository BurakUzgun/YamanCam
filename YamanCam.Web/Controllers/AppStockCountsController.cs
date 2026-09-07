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
/// Sayım Kaydı giriş ekranı. Bir şubede belirli bir dönem için (başlangıç - bitiş tarihi)
/// yapılan stok sayımını kaydeder. "Stokları Getir" ile aktif stoklar satır olarak yüklenir,
/// her satırda sayım miktarı girilir. Fiş üzerinde döviz/KDV/tutar yoktur. Yapı
/// AppStockOpeningsController ile aynıdır.
/// </summary>
public class AppStockCountsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;
    private readonly IUserRightService _userRightService;

    public AppStockCountsController(ApplicationDbContext context, IAppLogService appLogService, IUserRightService userRightService)
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

        var query = _context.AppStockCounts.AsNoTracking();

        query = showDeleted
            ? query.Where(x => x.IsActive == false)
            : query.Where(x => x.IsActive != false);

        var items = await query
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.RecId)
            .Select(x => new AppStockCountListItemViewModel
            {
                RecId = x.RecId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                WorkPlaceName = x.WorkPlace != null ? x.WorkPlace.WorkPlaceName : null,
                LineCount = x.Lines.Count
            })
            .ToListAsync();

        return View(new AppStockCountListViewModel
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

        var vm = new AppStockCountEditViewModel();
        await PopulateSelectListsAsync(vm);
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppStockCountEditViewModel vm)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        await ValidateCountAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            return View("Edit", vm);
        }

        var entity = new AppStockCount { IsActive = true };
        ApplyViewModel(entity, vm);
        entity.CreatedDate = DateTime.UtcNow;
        ApplyLines(entity, vm);

        _context.AppStockCounts.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "StockCount",
            "Created",
            $"Sayım kaydı oluşturuldu. RecId={entity.RecId}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Sayım kaydı kaydedildi.";
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

        var entity = await _context.AppStockCounts
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
    public async Task<IActionResult> Edit(int id, AppStockCountEditViewModel vm)
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

        var entity = await _context.AppStockCounts
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.RecId == id);

        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        await ValidateCountAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            return View(vm);
        }

        ApplyViewModel(entity, vm);
        _context.AppStockCountLines.RemoveRange(entity.Lines);
        entity.Lines.Clear();
        ApplyLines(entity, vm);

        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "StockCount",
            "Updated",
            $"Sayım kaydı güncellendi. RecId={entity.RecId}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Sayım kaydı güncellendi.";
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

        var entity = await _context.AppStockCounts
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
            "StockCount",
            "Deleted",
            $"Sayım kaydı silindi. RecId={entity.RecId}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Sayım kaydı silindi.";
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

        var entity = await _context.AppStockCounts
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
            "StockCount",
            "Restored",
            $"Sayım kaydı geri yüklendi. RecId={entity.RecId}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Sayım kaydı geri yüklendi.";
        return RedirectToAction(nameof(Index), new { showDeleted = true });
    }

    private async Task<IActionResult?> EnsureAdmin()
    {
        var action = AppScreenRights.ResolveAction(ControllerContext.ActionDescriptor.ActionName);
        var allowed = await _userRightService.HasScreenAccessAsync(User, ControllerContext.ActionDescriptor.ControllerName, action);
        return allowed ? null : RedirectToAction("Index", "Home");
    }

    private async Task PopulateSelectListsAsync(AppStockCountEditViewModel vm)
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
            .Select(x => new { x.RecId, x.StockCode, x.StockName })
            .ToListAsync();

        vm.StockOptions = stocks
            .Select(x => new SelectListItem($"{x.StockCode} - {x.StockName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();
    }

    private async Task ValidateCountAsync(AppStockCountEditViewModel vm)
    {
        if (vm.EndDate < vm.StartDate)
        {
            ModelState.AddModelError(nameof(vm.EndDate), "Bitiş tarihi başlangıç tarihinden önce olamaz.");
        }

        if (!vm.WorkPlaceId.HasValue ||
            !await _context.AppWorkPlaces.AnyAsync(x => x.RecId == vm.WorkPlaceId.Value))
        {
            ModelState.AddModelError(nameof(vm.WorkPlaceId), "Geçerli bir şube seçilmelidir.");
        }

        vm.Lines = vm.Lines
            .Where(x => x.StockId > 0 || x.CountQuantity != 0)
            .ToList();

        if (vm.Lines.Count < 1)
        {
            ModelState.AddModelError(string.Empty, "En az 1 sayım satırı girilmelidir.");
        }

        var seenStockIds = new HashSet<int>();

        for (var i = 0; i < vm.Lines.Count; i++)
        {
            var line = vm.Lines[i];
            line.LineNo = i + 1;

            if (line.StockId <= 0 || !await _context.AppStocks.AnyAsync(x => x.RecId == line.StockId))
            {
                ModelState.AddModelError($"Lines[{i}].StockId", "Geçerli bir stok seçilmelidir.");
            }
            else if (!seenStockIds.Add(line.StockId))
            {
                ModelState.AddModelError($"Lines[{i}].StockId", "Aynı stok birden fazla satırda olamaz.");
            }

            if (line.CountQuantity < 0)
            {
                ModelState.AddModelError($"Lines[{i}].CountQuantity", "Sayım miktarı negatif olamaz.");
            }
        }
    }

    private AppStockCountEditViewModel MapToViewModel(AppStockCount entity)
    {
        return new AppStockCountEditViewModel
        {
            RecId = entity.RecId,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            WorkPlaceId = entity.WorkPlaceId,
            Lines = entity.Lines
                .OrderBy(x => x.LineNo)
                .Select(x => new AppStockCountLineEditViewModel
                {
                    RecId = x.RecId,
                    LineNo = x.LineNo,
                    StockId = x.StockId,
                    CountQuantity = x.CountQuantity
                })
                .ToList()
        };
    }

    private static void ApplyViewModel(AppStockCount entity, AppStockCountEditViewModel vm)
    {
        entity.StartDate = vm.StartDate;
        entity.EndDate = vm.EndDate;
        entity.WorkPlaceId = vm.WorkPlaceId;
    }

    private static void ApplyLines(AppStockCount entity, AppStockCountEditViewModel vm)
    {
        foreach (var line in vm.Lines)
        {
            entity.Lines.Add(new AppStockCountLine
            {
                LineNo = line.LineNo,
                StockId = line.StockId,
                CountQuantity = line.CountQuantity,
                CreatedDate = DateTime.UtcNow
            });
        }
    }

    private static string BuildAuditValue(AppStockCount entity)
    {
        var sb = new StringBuilder();
        sb.Append($"Baslangic={entity.StartDate:yyyy-MM-dd}, Bitis={entity.EndDate:yyyy-MM-dd}");
        sb.Append($", SubeId={entity.WorkPlaceId}");
        sb.Append($", SatirSayisi={entity.Lines.Count}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
