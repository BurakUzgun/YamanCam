using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

public class AppSettingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;
    private readonly IUserRightService _userRightService;
    private readonly IAppSettingService _appSettingService;

    public AppSettingsController(
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
    public async Task<IActionResult> Index(bool showPassive = false)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var query = _context.AppSettings.AsNoTracking();

        if (showPassive)
        {
            query = query.Where(x => x.IsActive == false);
        }
        else
        {
            query = query.Where(x => x.IsActive != false);
        }

        var items = await query
            .OrderBy(x => x.SettingGroup)
            .ThenBy(x => x.RecId)
            .Select(x => new AppSettingListItemViewModel
            {
                RecId = x.RecId,
                SettingGroup = x.SettingGroup,
                Explanation = x.Explanation,
                Value = x.Value,
                IsActive = x.IsActive != false
            })
            .ToListAsync();

        return View(new AppSettingListViewModel
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

        return View("Edit", new AppSettingEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppSettingEditViewModel vm)
    {
        var denied = await EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        NormalizeCheckboxes(vm);
        ValidateSetting(vm);

        if (!ModelState.IsValid)
        {
            return View("Edit", vm);
        }

        var entity = MapToEntity(vm);
        entity.CreatedDate = DateTime.UtcNow;
        _context.AppSettings.Add(entity);
        await _context.SaveChangesAsync();
        _appSettingService.InvalidateCache();

        await _appLogService.WriteInfoAsync(
            "Setting",
            "Created",
            $"Ayar oluşturuldu. Grup={entity.SettingGroup}, Açıklama={entity.Explanation}",
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Ayar kaydedildi.";
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

        var entity = await _context.AppSettings.AsNoTracking().FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        return View(MapToViewModel(entity));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppSettingEditViewModel vm)
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
        var entity = await _context.AppSettings.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var oldAudit = BuildAuditValue(entity);
        ValidateSetting(vm);

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        ApplyViewModel(entity, vm);
        await _context.SaveChangesAsync();
        _appSettingService.InvalidateCache();

        await _appLogService.WriteInfoAsync(
            "Setting",
            "Updated",
            $"Ayar güncellendi. Grup={entity.SettingGroup}, Açıklama={entity.Explanation}",
            oldValue: oldAudit,
            newValue: BuildAuditValue(entity));

        TempData["SuccessMessage"] = "Ayar güncellendi.";
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

        var entity = await _context.AppSettings.FirstOrDefaultAsync(x => x.RecId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var explanation = entity.Explanation;
        var oldAudit = BuildAuditValue(entity);

        _context.AppSettings.Remove(entity);
        await _context.SaveChangesAsync();
        _appSettingService.InvalidateCache();

        await _appLogService.WriteInfoAsync(
            "Setting",
            "Deleted",
            $"Ayar silindi. Açıklama={explanation}",
            oldValue: oldAudit);

        TempData["SuccessMessage"] = "Ayar silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult?> EnsureAdmin()
    {
        var action = AppScreenRights.ResolveAction(ControllerContext.ActionDescriptor.ActionName);
        var allowed = await _userRightService.HasScreenAccessAsync(User, ControllerContext.ActionDescriptor.ControllerName, action);
        return allowed ? null : RedirectToAction("Index", "Home");
    }

    private void NormalizeCheckboxes(AppSettingEditViewModel vm)
    {
        vm.IsActive = string.Equals(Request.Form["IsActive"], "true", StringComparison.Ordinal);
    }

    private void ValidateSetting(AppSettingEditViewModel vm)
    {
        vm.SettingGroup = vm.SettingGroup.Trim();
        vm.Explanation = vm.Explanation.Trim();
        vm.Value = vm.Value.Trim();
    }

    private static AppSettingEditViewModel MapToViewModel(AppSetting entity)
    {
        return new AppSettingEditViewModel
        {
            RecId = entity.RecId,
            SettingGroup = entity.SettingGroup,
            Explanation = entity.Explanation,
            Value = entity.Value,
            IsActive = entity.IsActive != false
        };
    }

    private static AppSetting MapToEntity(AppSettingEditViewModel vm)
    {
        var entity = new AppSetting();
        ApplyViewModel(entity, vm);
        return entity;
    }

    private static void ApplyViewModel(AppSetting entity, AppSettingEditViewModel vm)
    {
        entity.SettingGroup = vm.SettingGroup.Trim();
        entity.Explanation = vm.Explanation.Trim();
        entity.Value = vm.Value;
        entity.IsActive = vm.IsActive;
    }

    private static string BuildAuditValue(AppSetting entity)
    {
        var sb = new StringBuilder();
        sb.Append($"Grup={entity.SettingGroup}, Açıklama={entity.Explanation}, Değer={entity.Value}");
        sb.Append($", IsActive={entity.IsActive == true}");
        return sb.ToString();
    }
}
