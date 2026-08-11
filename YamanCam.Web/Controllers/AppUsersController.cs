using System.Text;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Controllers;

public class AppUsersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogService _appLogService;

    public AppUsersController(ApplicationDbContext context, IAppLogService appLogService)
    {
        _context = context;
        _appLogService = appLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var users = await _context.AppUsers
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new AppUserListItemViewModel
            {
                RecId = x.RecId,
                Code = x.Code,
                NameSurname = x.NameSurname,
                IsRight = x.IsRight,
                IsActive = x.IsActive
            })
            .ToListAsync();

        var workPlaceLinks = await _context.AppUserWorkPlaces
            .AsNoTracking()
            .Where(x => x.IsActive == true)
            .Join(
                _context.AppWorkPlaces.AsNoTracking(),
                link => link.WorkPlaceId,
                wp => wp.RecId,
                (link, wp) => new { link.UserId, wp.WorkPlaceCode, wp.WorkPlaceName })
            .ToListAsync();

        var summaryByUser = workPlaceLinks
            .GroupBy(x => x.UserId)
            .ToDictionary(
                g => g.Key,
                g => string.Join(", ", g.Select(x => x.WorkPlaceCode ?? x.WorkPlaceName ?? "-")));

        foreach (var user in users)
        {
            user.WorkPlaceSummary = summaryByUser.GetValueOrDefault(user.RecId, "-");
        }

        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        var vm = await BuildEditViewModelAsync(null);
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppUserEditViewModel vm)
    {
        var denied = EnsureAdmin();
        if (denied is not null)
        {
            return denied;
        }

        vm.RecId = 0;
        vm.SelectedWorkPlaceIds = ParseSelectedWorkPlaceIds();
        NormalizeCheckboxes(vm);

        if (string.IsNullOrWhiteSpace(vm.Password))
        {
            ModelState.AddModelError(nameof(vm.Password), "Yeni kullanıcı için parola zorunludur.");
        }

        await ValidateUserAsync(vm);
        if (!ModelState.IsValid)
        {
            vm.WorkPlaceOptions = await LoadWorkPlaceOptionsAsync(vm.SelectedWorkPlaceIds);
            return View("Edit", vm);
        }

        var user = new AppUser
        {
            Code = vm.Code.Trim(),
            NameSurname = vm.NameSurname.Trim(),
            Password = vm.Password,
            Email = vm.Email?.Trim(),
            Phone = vm.Phone?.Trim(),
            IsRight = vm.IsRight,
            IsActive = vm.IsActive,
            CreatedDate = DateTime.UtcNow
        };

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();

        await SyncUserWorkPlacesAsync(user.RecId, vm.SelectedWorkPlaceIds, vm.DefaultWorkPlaceId);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "User",
            "Created",
            $"Kullanıcı oluşturuldu. Code={user.Code}",
            newValue: BuildUserAuditValue(user, vm.SelectedWorkPlaceIds, vm.DefaultWorkPlaceId));

        TempData["SuccessMessage"] = "Kullanıcı kaydedildi.";
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

        var vm = await BuildEditViewModelAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppUserEditViewModel vm)
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

        vm.SelectedWorkPlaceIds = ParseSelectedWorkPlaceIds();
        NormalizeCheckboxes(vm);

        var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.RecId == id);
        if (user is null)
        {
            return NotFound();
        }

        var oldAudit = BuildUserAuditValue(
            user,
            await GetSelectedWorkPlaceIdsAsync(user.RecId),
            await GetDefaultWorkPlaceIdAsync(user.RecId));

        await ValidateUserAsync(vm);
        if (!ModelState.IsValid)
        {
            vm.WorkPlaceOptions = await LoadWorkPlaceOptionsAsync(vm.SelectedWorkPlaceIds);
            return View(vm);
        }

        user.Code = vm.Code.Trim();
        user.NameSurname = vm.NameSurname.Trim();
        if (!string.IsNullOrWhiteSpace(vm.Password))
        {
            user.Password = vm.Password;
        }

        user.Email = vm.Email?.Trim();
        user.Phone = vm.Phone?.Trim();
        user.IsRight = vm.IsRight;
        user.IsActive = vm.IsActive;

        await SyncUserWorkPlacesAsync(user.RecId, vm.SelectedWorkPlaceIds, vm.DefaultWorkPlaceId);
        await _context.SaveChangesAsync();

        await _appLogService.WriteInfoAsync(
            "User",
            "Updated",
            $"Kullanıcı güncellendi. Code={user.Code}",
            oldValue: oldAudit,
            newValue: BuildUserAuditValue(user, vm.SelectedWorkPlaceIds, vm.DefaultWorkPlaceId));

        TempData["SuccessMessage"] = "Kullanıcı güncellendi.";
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

    private List<int> ParseSelectedWorkPlaceIds()
    {
        return Request.Form["SelectedWorkPlaceIds"]
            .Select(value => int.TryParse(value, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
    }

    private void NormalizeCheckboxes(AppUserEditViewModel vm)
    {
        vm.IsRight = string.Equals(Request.Form["IsRight"], "true", StringComparison.Ordinal);
        vm.IsActive = string.Equals(Request.Form["IsActive"], "true", StringComparison.Ordinal);

        if (int.TryParse(Request.Form["DefaultWorkPlaceId"], out var defaultId) && defaultId > 0)
        {
            vm.DefaultWorkPlaceId = defaultId;
        }
        else
        {
            vm.DefaultWorkPlaceId = vm.SelectedWorkPlaceIds.FirstOrDefault();
        }
    }

    private async Task ValidateUserAsync(AppUserEditViewModel vm)
    {
        var codeExists = await _context.AppUsers.AnyAsync(x =>
            x.Code == vm.Code.Trim() && x.RecId != vm.RecId);
        if (codeExists)
        {
            ModelState.AddModelError(nameof(vm.Code), "Bu kullanıcı kodu zaten kullanılıyor.");
        }

        if (vm.SelectedWorkPlaceIds.Count == 0 && !vm.IsRight)
        {
            ModelState.AddModelError(string.Empty, "Yönetici olmayan kullanıcılar için en az bir şube seçilmelidir.");
        }

        if (vm.DefaultWorkPlaceId.HasValue && !vm.SelectedWorkPlaceIds.Contains(vm.DefaultWorkPlaceId.Value))
        {
            ModelState.AddModelError(nameof(vm.DefaultWorkPlaceId), "Varsayılan şube, seçili şubeler arasında olmalıdır.");
        }

        vm.WorkPlaceOptions = await LoadWorkPlaceOptionsAsync(vm.SelectedWorkPlaceIds);
    }

    private async Task<AppUserEditViewModel?> BuildEditViewModelAsync(int? userId)
    {
        AppUser? user = null;
        List<int> selectedIds = [];

        if (userId.HasValue)
        {
            user = await _context.AppUsers.AsNoTracking().FirstOrDefaultAsync(x => x.RecId == userId.Value);
            if (user is null)
            {
                return null;
            }

            selectedIds = await GetSelectedWorkPlaceIdsAsync(user.RecId);
        }

        var defaultWorkPlaceId = user is null
            ? null
            : await GetDefaultWorkPlaceIdAsync(user.RecId);

        return new AppUserEditViewModel
        {
            RecId = user?.RecId ?? 0,
            Code = user?.Code ?? string.Empty,
            NameSurname = user?.NameSurname ?? string.Empty,
            Email = user?.Email,
            Phone = user?.Phone,
            IsRight = user?.IsRight == true,
            IsActive = user?.IsActive != false,
            DefaultWorkPlaceId = defaultWorkPlaceId ?? selectedIds.FirstOrDefault(),
            SelectedWorkPlaceIds = selectedIds,
            WorkPlaceOptions = await LoadWorkPlaceOptionsAsync(selectedIds)
        };
    }

    private async Task<List<UserWorkPlaceOptionViewModel>> LoadWorkPlaceOptionsAsync(IReadOnlyCollection<int> selectedIds)
    {
        var selectedSet = selectedIds.ToHashSet();
        return await _context.AppWorkPlaces
            .AsNoTracking()
            .Where(x => x.IsActive == true)
            .OrderBy(x => x.WorkPlaceCode)
            .Select(x => new UserWorkPlaceOptionViewModel
            {
                WorkPlaceId = x.RecId,
                WorkPlaceCode = x.WorkPlaceCode,
                WorkPlaceName = x.WorkPlaceName,
                IsSelected = selectedSet.Contains(x.RecId)
            })
            .ToListAsync();
    }

    private async Task<List<int>> GetSelectedWorkPlaceIdsAsync(int userId)
    {
        return await _context.AppUserWorkPlaces
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive == true)
            .Select(x => x.WorkPlaceId)
            .ToListAsync();
    }

    private async Task<int?> GetDefaultWorkPlaceIdAsync(int userId)
    {
        return await _context.AppUserWorkPlaces
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive == true && x.IsDefault == true)
            .Select(x => (int?)x.WorkPlaceId)
            .FirstOrDefaultAsync();
    }

    private async Task SyncUserWorkPlacesAsync(int userId, IReadOnlyList<int> selectedWorkPlaceIds, int? defaultWorkPlaceId)
    {
        var selectedSet = selectedWorkPlaceIds.Distinct().ToHashSet();
        if (defaultWorkPlaceId.HasValue && !selectedSet.Contains(defaultWorkPlaceId.Value))
        {
            defaultWorkPlaceId = selectedSet.FirstOrDefault();
        }

        var existing = await _context.AppUserWorkPlaces
            .Where(x => x.UserId == userId)
            .ToListAsync();

        foreach (var link in existing.Where(x => !selectedSet.Contains(x.WorkPlaceId)))
        {
            _context.AppUserWorkPlaces.Remove(link);
        }

        foreach (var workPlaceId in selectedSet)
        {
            var link = existing.FirstOrDefault(x => x.WorkPlaceId == workPlaceId);
            if (link is null)
            {
                link = new AppUserWorkPlace
                {
                    UserId = userId,
                    WorkPlaceId = workPlaceId,
                    CreatedDate = DateTime.UtcNow
                };
                _context.AppUserWorkPlaces.Add(link);
            }

            link.IsActive = true;
            link.IsDefault = defaultWorkPlaceId == workPlaceId;
        }

        var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.RecId == userId);
        if (user is not null)
        {
            user.HotelsId = selectedSet.Count == 0 ? null : string.Join(",", selectedSet);
        }
    }

    private static string BuildUserAuditValue(AppUser user, IReadOnlyCollection<int> workPlaceIds, int? defaultWorkPlaceId)
    {
        var sb = new StringBuilder();
        sb.Append($"Code={user.Code}, Name={user.NameSurname}, IsRight={user.IsRight == true}, IsActive={user.IsActive == true}");
        sb.Append($", Subeler=[{string.Join(",", workPlaceIds)}], VarsayilanSube={defaultWorkPlaceId?.ToString() ?? "-"}");
        return sb.ToString();
    }
}
