using System.Globalization;
using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

public class AppProductionDefinitionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;
    private readonly IUserRightService _userRightService;

    public AppProductionDefinitionsController(ApplicationDbContext context, IAppLogService appLogService, IUserRightService userRightService)
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

        var query = _context.AppProductionDefinitions
            .AsNoTracking()
            .Include(x => x.ProductStock)
            .Include(x => x.RawMaterialStock)
            .Include(x => x.WorkPlace)
            .AsQueryable();

        query = showPassive
            ? query.Where(x => x.IsActive == false)
            : query.Where(x => x.IsActive != false);

        var items = await query
            .OrderBy(x => x.ProductStock!.StockName)
            .Select(x => new AppProductionDefinitionListItemViewModel
            {
                RecId = x.RecId,
                ProductStockName = x.ProductStock != null ? x.ProductStock.StockName : string.Empty,
                RawMaterialStockName = x.RawMaterialStock != null ? x.RawMaterialStock.StockName : string.Empty,
                ProductionQuantity = x.ProductionQuantity,
                WorkPlaceName = x.WorkPlace != null ? x.WorkPlace.WorkPlaceName ?? string.Empty : string.Empty,
                IsActive = x.IsActive != false
            })
            .ToListAsync();

        return View(new AppProductionDefinitionListViewModel
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

        var vm = new AppProductionDefinitionEditViewModel();
        await PopulateSelectListsAsync(vm);
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppProductionDefinitionEditViewModel vm)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        NormalizeCheckboxes(vm);
        await ValidateProductionDefinitionAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            return View("Edit", vm);
        }

        var entity = new AppProductionDefinition { IsActive = true };
        ApplyViewModel(entity, vm);
        entity.CreatedDate = DateTime.UtcNow;

        _context.AppProductionDefinitions.Add(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "ProductionDefinition",
            "Created",
            $"Üretim tanımı oluşturuldu. RecId={entity.RecId}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Üretim tanımı kaydedildi.";
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

        var entity = await _context.AppProductionDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.RecId == id);
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
    public async Task<IActionResult> Edit(int id, AppProductionDefinitionEditViewModel vm)
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

        var entity = await _context.AppProductionDefinitions.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        NormalizeCheckboxes(vm);
        await ValidateProductionDefinitionAsync(vm);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(vm);
            return View(vm);
        }

        ApplyViewModel(entity, vm);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "ProductionDefinition",
            "Updated",
            $"Üretim tanımı güncellendi. RecId={entity.RecId}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Üretim tanımı güncellendi.";
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

        var entity = await _context.AppProductionDefinitions.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);

        _context.AppProductionDefinitions.Remove(entity);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "ProductionDefinition",
            "Deleted",
            $"Üretim tanımı silindi. RecId={entity.RecId}",
            oldValue: oldAudit);

        TempData["SuccessMessage"] = "Üretim tanımı silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult?> EnsureAdmin()
    {
        var action = AppScreenRights.ResolveAction(ControllerContext.ActionDescriptor.ActionName);
        var allowed = await _userRightService.HasScreenAccessAsync(User, ControllerContext.ActionDescriptor.ControllerName, action);
        return allowed ? null : RedirectToAction("Index", "Home");
    }

    private void NormalizeCheckboxes(AppProductionDefinitionEditViewModel vm)
    {
        vm.IsActive = string.Equals(Request.Form["IsActive"], "true", StringComparison.Ordinal);
    }

    private async Task PopulateSelectListsAsync(AppProductionDefinitionEditViewModel vm)
    {
        var stocks = await _context.AppStocks.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.StockCode)
            .Select(x => new { x.RecId, x.StockCode, x.StockName })
            .ToListAsync();

        vm.StockOptions = stocks
            .Select(x => new SelectListItem($"{x.StockCode} - {x.StockName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var workPlaces = await _context.AppWorkPlaces.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.WorkPlaceCode)
            .Select(x => new { x.RecId, x.WorkPlaceCode, x.WorkPlaceName })
            .ToListAsync();

        vm.WorkPlaceOptions = workPlaces
            .Select(x => new SelectListItem($"{x.WorkPlaceCode} - {x.WorkPlaceName}", x.RecId.ToString(CultureInfo.InvariantCulture)))
            .ToList();
    }

    private async Task ValidateProductionDefinitionAsync(AppProductionDefinitionEditViewModel vm)
    {
        if (vm.ProductStockId <= 0 || !await _context.AppStocks.AnyAsync(x => x.RecId == vm.ProductStockId))
        {
            ModelState.AddModelError(nameof(vm.ProductStockId), "Geçerli bir mamul seçilmelidir.");
        }

        if (vm.RawMaterialStockId <= 0 || !await _context.AppStocks.AnyAsync(x => x.RecId == vm.RawMaterialStockId))
        {
            ModelState.AddModelError(nameof(vm.RawMaterialStockId), "Geçerli bir hammadde seçilmelidir.");
        }

        if (vm.ProductStockId > 0 && vm.ProductStockId == vm.RawMaterialStockId)
        {
            ModelState.AddModelError(nameof(vm.RawMaterialStockId), "Mamul ve hammadde aynı stok olamaz.");
        }

        if (!await _context.AppWorkPlaces.AnyAsync(x => x.RecId == vm.WorkPlaceId))
        {
            ModelState.AddModelError(nameof(vm.WorkPlaceId), "Geçerli bir şube seçilmelidir.");
        }

        var duplicateExists = await _context.AppProductionDefinitions.AnyAsync(x =>
            x.ProductStockId == vm.ProductStockId &&
            x.RawMaterialStockId == vm.RawMaterialStockId &&
            x.WorkPlaceId == vm.WorkPlaceId &&
            x.RecId != vm.RecId);
        if (duplicateExists)
        {
            ModelState.AddModelError(string.Empty, "Bu mamul, hammadde ve şube için zaten bir üretim tanımı mevcut.");
        }
    }

    private static AppProductionDefinitionEditViewModel MapToViewModel(AppProductionDefinition entity)
    {
        return new AppProductionDefinitionEditViewModel
        {
            RecId = entity.RecId,
            ProductStockId = entity.ProductStockId,
            RawMaterialStockId = entity.RawMaterialStockId,
            ProductionQuantity = entity.ProductionQuantity,
            WorkPlaceId = entity.WorkPlaceId,
            IsActive = entity.IsActive != false
        };
    }

    private static void ApplyViewModel(AppProductionDefinition entity, AppProductionDefinitionEditViewModel vm)
    {
        entity.ProductStockId = vm.ProductStockId;
        entity.RawMaterialStockId = vm.RawMaterialStockId;
        entity.ProductionQuantity = vm.ProductionQuantity;
        entity.WorkPlaceId = vm.WorkPlaceId;
        entity.IsActive = vm.IsActive;
    }

    private static string BuildAuditValue(AppProductionDefinition entity)
    {
        var sb = new StringBuilder();
        sb.Append($"MamulId={entity.ProductStockId}, HammaddeId={entity.RawMaterialStockId}");
        sb.Append($", Miktar={entity.ProductionQuantity}, SubeId={entity.WorkPlaceId}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
