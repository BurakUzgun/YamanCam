using YamanCam.Web.Models;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace YamanCam.Web.Controllers;

public class SqlConnectionController : Controller
{
    private readonly ISqlConnectionSettingsService _connectionSettingsService;
    private readonly IAppLogService _appLogService;

    public SqlConnectionController(
        ISqlConnectionSettingsService connectionSettingsService,
        IAppLogService appLogService)
    {
        _connectionSettingsService = connectionSettingsService;
        _appLogService = appLogService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var settings = _connectionSettingsService.GetSettings();
        var vm = new SqlConnectionViewModel
        {
            Server = settings.Server,
            Database = settings.Database,
            UseTrustedConnection = settings.UseTrustedConnection,
            UserId = settings.UserId,
            Password = settings.Password,
            TrustServerCertificate = settings.TrustServerCertificate
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SqlConnectionViewModel vm)
    {
        var currentSettings = _connectionSettingsService.GetSettings();

        if (!vm.UseTrustedConnection)
        {
            if (string.IsNullOrWhiteSpace(vm.UserId))
            {
                ModelState.AddModelError(nameof(vm.UserId), "User Id zorunludur.");
            }

            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                ModelState.AddModelError(nameof(vm.Password), "Password zorunludur.");
            }
        }

        if (!ModelState.IsValid)
        {
            await _appLogService.WriteErrorAsync("SqlConnection", "ValidationFailed", "SQL baglanti formu dogrulama hatasi.");
            return View(vm);
        }

        var settings = new SqlConnectionSettings
        {
            Server = vm.Server.Trim(),
            Database = vm.Database.Trim(),
            UseTrustedConnection = vm.UseTrustedConnection,
            UserId = vm.UserId?.Trim(),
            Password = vm.Password,
            TrustServerCertificate = vm.TrustServerCertificate
        };

        await _connectionSettingsService.SaveSettingsAsync(settings);

        try
        {
            await using var connection = new SqlConnection(_connectionSettingsService.BuildConnectionString());
            await connection.OpenAsync();
            TempData["SuccessMessage"] = "SQL baglantisi basariyla kaydedildi.";
            await _appLogService.WriteInfoAsync(
                "SqlConnection",
                "Saved",
                $"SQL baglantisi kaydedildi. Server={settings.Server}, Database={settings.Database}",
                oldValue: BuildSqlSettingsAuditValue(currentSettings),
                newValue: BuildSqlSettingsAuditValue(settings));
            return RedirectToAction("Status", "License");
        }
        catch (Exception ex)
        {
            await _appLogService.WriteErrorAsync("SqlConnection", "ConnectionFailed", $"SQL baglanti testi basarisiz: {ex.Message}");
            ModelState.AddModelError(string.Empty, $"Baglanti kurulamadi: {ex.Message}");
            return View(vm);
        }
    }

    private static string BuildSqlSettingsAuditValue(SqlConnectionSettings settings)
    {
        var userPart = settings.UseTrustedConnection
            ? "TrustedConnection"
            : $"UserId={settings.UserId ?? "-"}, PasswordSet={(string.IsNullOrWhiteSpace(settings.Password) ? "Hayir" : "Evet")}";

        return $"Server={settings.Server}, Database={settings.Database}, {userPart}, TrustServerCertificate={settings.TrustServerCertificate}";
    }
}
