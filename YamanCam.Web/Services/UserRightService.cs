using System.Security.Claims;
using YamanCam.Web.Data;
using YamanCam.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Services;

public class UserRightService : IUserRightService
{
    private readonly ApplicationDbContext _context;

    public UserRightService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasScreenAccessAsync(ClaimsPrincipal user, string controllerCode, ScreenActionType action)
    {
        var appUser = await GetCurrentAppUserAsync(user);
        if (appUser is null)
        {
            return false;
        }

        if (appUser.IsRight == true)
        {
            return true;
        }

        var rightCode = AppScreenRights.BuildRightCode(controllerCode, action);
        return await _context.AppUserRights.AsNoTracking().AnyAsync(x =>
            x.UserId == appUser.RecId &&
            x.RightCode == rightCode &&
            x.IsActive == true &&
            x.IsAllowed == true);
    }

    public async Task<HashSet<string>> GetAllowedScreenCodesAsync(ClaimsPrincipal user)
    {
        var appUser = await GetCurrentAppUserAsync(user);
        if (appUser is null)
        {
            return [];
        }

        if (appUser.IsRight == true)
        {
            return AppScreenRights.All.Select(x => x.Code).ToHashSet();
        }

        var viewRightCodes = AppScreenRights.All
            .Select(x => AppScreenRights.BuildRightCode(x.Code, ScreenActionType.View))
            .ToHashSet();

        var allowedViewCodes = await _context.AppUserRights
            .AsNoTracking()
            .Where(x => x.UserId == appUser.RecId && x.IsActive == true && x.IsAllowed == true && viewRightCodes.Contains(x.RightCode))
            .Select(x => x.RightCode)
            .ToListAsync();

        return allowedViewCodes.Select(x => x[..x.LastIndexOf('.')]).ToHashSet();
    }

    private async Task<AppUser?> GetCurrentAppUserAsync(ClaimsPrincipal user)
    {
        if (!int.TryParse(user.FindFirst("UserId")?.Value, out var userId))
        {
            return null;
        }

        return await _context.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RecId == userId && x.IsActive == true);
    }
}
