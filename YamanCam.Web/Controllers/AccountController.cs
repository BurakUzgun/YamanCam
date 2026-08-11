using YamanCam.Web.Data;
using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace YamanCam.Web.Controllers;

public class AccountController : Controller
{
    private const string RememberMeCookieName = "YamanCam.RememberMe";

    private static readonly JsonSerializerOptions RememberCookieJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly ApplicationDbContext _context;
    private readonly IDataProtector _rememberMeProtector;
    private readonly IAppLogService _appLogService;

    public AccountController(
        ApplicationDbContext context,
        IDataProtectionProvider dataProtectionProvider,
        IAppLogService appLogService)
    {
        _context = context;
        _rememberMeProtector = dataProtectionProvider.CreateProtector("YamanCam.RememberMeCookie.v1");
        _appLogService = appLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        var vm = new LoginViewModel();
        if (Request.Cookies.TryGetValue(RememberMeCookieName, out var encryptedPayload) &&
            !string.IsNullOrWhiteSpace(encryptedPayload))
        {
            try
            {
                var plaintext = _rememberMeProtector.Unprotect(encryptedPayload);
                if (!TryApplyRememberCookiePayload(vm, plaintext))
                {
                    Response.Cookies.Delete(RememberMeCookieName);
                }
            }
            catch
            {
                Response.Cookies.Delete(RememberMeCookieName);
            }
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        if (vm.RememberMe || vm.RememberPassword)
        {
            var dto = new LoginRememberCookieDto
            {
                Username = vm.Username ?? string.Empty,
                Password = vm.RememberPassword ? (vm.Password ?? string.Empty) : string.Empty,
                RememberMe = vm.RememberMe,
                RememberPassword = vm.RememberPassword
            };
            var payload = JsonSerializer.Serialize(dto, RememberCookieJsonOptions);
            var encryptedPayload = _rememberMeProtector.Protect(payload);
            Response.Cookies.Append(RememberMeCookieName, encryptedPayload, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });
        }
        else
        {
            Response.Cookies.Delete(RememberMeCookieName);
        }

        var appUser = await _context.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive == true &&
                                      (x.Code == vm.Username || x.NameSurname == vm.Username));

        if (appUser is null || !string.Equals(appUser.Password, vm.Password, StringComparison.Ordinal))
        {
            await _appLogService.WriteErrorAsync("Auth", "LoginFailed", $"Giris basarisiz. Username={vm.Username}");
            ModelState.AddModelError(string.Empty, "Kullanici kodu veya sifre yanlistir.");
            return View(vm);
        }
        var currentUser = appUser;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, vm.Username ?? string.Empty),
            new("IsRight", currentUser.IsRight == true ? "true" : "false")
        };

        claims.Add(new("UserId", currentUser.RecId.ToString()));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = false,
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        var loggedUser = currentUser.Code ?? currentUser.NameSurname ?? vm.Username;
        await _appLogService.WriteInfoAsync("Auth", "LoginSuccess", $"Giris basarili. Username={loggedUser}");

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userName = User.Identity?.Name;
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await _appLogService.WriteInfoAsync("Auth", "Logout", $"Cikis yapildi. Username={userName}");
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword()
    {
        var appUser = await GetCurrentAppUserAsync();
        if (appUser is null)
        {
            return RedirectToAction("Login");
        }

        return View(new ChangePasswordViewModel
        {
            Code = appUser.Code ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        var appUser = await GetCurrentAppUserAsync();
        if (appUser is null)
        {
            return RedirectToAction("Login");
        }

        vm.Code = appUser.Code ?? string.Empty;

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        if (!string.Equals(appUser.Password, vm.CurrentPassword, StringComparison.Ordinal))
        {
            await _appLogService.WriteErrorAsync("Auth", "ChangePasswordFailed", $"Mevcut sifre yanlis. User={appUser.Code}");
            ModelState.AddModelError(string.Empty, "Kullanici kodu veya sifre yanlistir.");
            return View(vm);
        }

        appUser.Password = vm.NewPassword;
        await _context.SaveChangesAsync();
        await _appLogService.WriteInfoAsync(
            "Auth",
            "ChangePasswordSuccess",
            $"Sifre degistirildi. User={appUser.Code}",
            oldValue: "Parola: [Gizli]",
            newValue: "Parola: [Gizli]");

        if (Request.Cookies.ContainsKey(RememberMeCookieName))
        {
            Response.Cookies.Delete(RememberMeCookieName);
        }

        TempData["SuccessMessage"] = "Sifreniz degistirilmistir.";
        return RedirectToAction(nameof(ChangePassword));
    }

    private async Task<AppUser?> GetCurrentAppUserAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        if (int.TryParse(User.FindFirst("UserId")?.Value, out var userId))
        {
            return await _context.AppUsers
                .FirstOrDefaultAsync(x => x.RecId == userId && x.IsActive == true);
        }

        var username = User.Identity.Name ?? string.Empty;
        return await _context.AppUsers
            .FirstOrDefaultAsync(x => x.IsActive == true &&
                                      (x.Code == username || x.NameSurname == username));
    }

    private static bool TryApplyRememberCookiePayload(LoginViewModel vm, string plaintext)
    {
        var trimmed = plaintext.TrimStart();
        if (trimmed.StartsWith('{'))
        {
            LoginRememberCookieDto? dto;
            try
            {
                dto = JsonSerializer.Deserialize<LoginRememberCookieDto>(plaintext, RememberCookieJsonOptions);
            }
            catch
            {
                return false;
            }

            if (dto is null)
            {
                return false;
            }

            vm.RememberMe = dto.RememberMe;
            vm.RememberPassword = dto.RememberPassword;
            if (dto.RememberMe)
            {
                vm.Username = dto.Username ?? string.Empty;
            }

            if (dto.RememberPassword)
            {
                vm.Password = dto.Password ?? string.Empty;
            }

            return true;
        }

        var parts = plaintext.Split('\n');
        if (parts.Length == 2)
        {
            vm.Username = parts[0];
            vm.Password = parts[1];
            vm.RememberMe = true;
            vm.RememberPassword = true;
            return true;
        }

        return false;
    }

    private sealed class LoginRememberCookieDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public bool RememberPassword { get; set; }
    }
}
