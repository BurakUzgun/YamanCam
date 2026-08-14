using System.Security.Claims;
using YamanCam.Web.Models;

namespace YamanCam.Web.Services;

public interface IUserRightService
{
    Task<bool> HasScreenAccessAsync(ClaimsPrincipal user, string controllerCode, ScreenActionType action);

    Task<HashSet<string>> GetAllowedScreenCodesAsync(ClaimsPrincipal user);
}
