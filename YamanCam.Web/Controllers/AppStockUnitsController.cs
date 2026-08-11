using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

public class AppStockUnitsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;

    public AppStockUnitsController(ApplicationDbContext context, IAppLogService appLogService)
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

        var query = _context.AppStockUnits.AsNoTracking();

        if (showPassive)
        {
            query = query.Where(x => x.IsActive == false);
        }
        else
        {
            query = query.Where(x => x.IsActive != false);
        }

        var items = await query
            .OrderBy(x => x.UnitCode)
            .Select(x => new AppStockUnitListItemViewModel
            {
                RecId = x.RecId,
                UnitCode = x.UnitCode,
                UnitName = x.UnitName,
                IsActive = x.IsActive != false
            })
            .ToListAsync();

        return View(new AppStockUnitListViewModel
        {
            ShowPassive = showPassive,
            Items = items
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        return View("Edit", new AppStockUnitEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppStockUnitEditViewModel vm)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        NormalizeCheckboxes(vm);
        await ValidateStockUnitAsync(vm);

        if (!ModelState.IsValid)
        {
            return View("Edit", vm);
        }

        var entity = MapToEntity(vm);
        entity.CreatedDate = DateTime.UtcNow;
        _context.AppStockUnits.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "StockUnit",
            "Created",
            $"Stok birimi oluşturuldu. Code={entity.UnitCode}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Stok birimi kaydedildi.";
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

        var entity = await _context.AppStockUnits.AsNoTracking().FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        return View(MapToViewModel(entity));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppStockUnitEditViewModel vm)
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
        var entity = await _context.AppStockUnits.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        await ValidateStockUnitAsync(vm);

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        ApplyViewModel(entity, vm);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "StockUnit",
            "Updated",
            $"Stok birimi güncellendi. Code={entity.UnitCode}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Stok birimi güncellendi.";
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

        var entity = await _context.AppStockUnits.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var unitCode = entity.UnitCode;
        var oldAudit = BuildAuditValue(entity);

        _context.AppStockUnits.Remove(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "StockUnit",
            "Deleted",
            $"Stok birimi silindi. Code={unitCode}",
            oldValue: oldAudit);

        TempData["SuccessMessage"] = "Stok birimi silindi.";
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

    private void NormalizeCheckboxes(AppStockUnitEditViewModel vm)
    {
        vm.IsActive = string.Equals(Request.Form["IsActive"], "true", StringComparison.Ordinal);
    }

    private async Task ValidateStockUnitAsync(AppStockUnitEditViewModel vm)
    {
        var code = vm.UnitCode.Trim().ToUpperInvariant();
        vm.UnitCode = code;

        var codeExists = await _context.AppStockUnits.AnyAsync(x =>
            x.UnitCode == code && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.UnitCode), "Bu birim kodu zaten kullanılıyor.");
        }
    }

    private static AppStockUnitEditViewModel MapToViewModel(AppStockUnit entity)
    {
        return new AppStockUnitEditViewModel
        {
            RecId = entity.RecId,
            UnitCode = entity.UnitCode,
            UnitName = entity.UnitName,
            IsActive = entity.IsActive != false
        };
    }

    private static AppStockUnit MapToEntity(AppStockUnitEditViewModel vm)
    {
        var entity = new AppStockUnit();
        ApplyViewModel(entity, vm);
        return entity;
    }

    private static void ApplyViewModel(AppStockUnit entity, AppStockUnitEditViewModel vm)
    {
        entity.UnitCode = vm.UnitCode.Trim().ToUpperInvariant();
        entity.UnitName = vm.UnitName.Trim();
        entity.IsActive = vm.IsActive;
    }

    private static string BuildAuditValue(AppStockUnit entity)
    {
        var sb = new StringBuilder();
        sb.Append($"Code={entity.UnitCode}, Name={entity.UnitName}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
