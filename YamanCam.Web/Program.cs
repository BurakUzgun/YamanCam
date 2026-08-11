using YamanCam.Web.Data;
using YamanCam.Web.Middleware;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "YamanCam.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(3650);
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    });
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});
builder.Services.AddSingleton<ISqlConnectionSettingsService, SqlConnectionSettingsService>();
builder.Services.AddSingleton<IDatabaseBootstrapService, DatabaseBootstrapService>();
builder.Services.AddSingleton<ICenterLicenseConnectionService, CenterLicenseConnectionService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAppLogService, AppLogService>();
builder.Services.AddHostedService<LicenseDailyCheckHostedService>();
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var sqlSettingsService = serviceProvider.GetRequiredService<ISqlConnectionSettingsService>();
    options.UseSqlServer(sqlSettingsService.BuildConnectionString());
});

var app = builder.Build();
var licenseConnectionService = app.Services.GetRequiredService<ICenterLicenseConnectionService>();
licenseConnectionService.EnsureConnectionFileExists();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.Use(async (context, next) =>
{
    var centerLicenseService = context.RequestServices.GetRequiredService<ICenterLicenseConnectionService>();
    var licenseStatus = await centerLicenseService.RefreshStatusIfNeededAsync(context.RequestAborted);

    var sqlSettingsService = context.RequestServices.GetRequiredService<ISqlConnectionSettingsService>();
    var databaseBootstrapService = context.RequestServices.GetRequiredService<IDatabaseBootstrapService>();
    var path = context.Request.Path.Value ?? string.Empty;
    var isSqlConnectionPath = path.StartsWith("/SqlConnection", StringComparison.OrdinalIgnoreCase);
    var isLoginPath = path.StartsWith("/Account/Login", StringComparison.OrdinalIgnoreCase);
    var isLogoutPath = path.StartsWith("/Account/Logout", StringComparison.OrdinalIgnoreCase);
    var isLicensePath = path.StartsWith("/License", StringComparison.OrdinalIgnoreCase);
    var isStaticPath = path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase);
    var sqlConfigured = sqlSettingsService.HasValidConfiguration();
    var isLoggedIn = context.User.Identity?.IsAuthenticated == true;
    var hasRight = string.Equals(context.User.FindFirst("IsRight")?.Value, "true", StringComparison.Ordinal);
    context.Items["HasRight"] = hasRight;

    if (!sqlConfigured && !isSqlConnectionPath && !isStaticPath)
    {
        context.Response.Redirect("/SqlConnection");
        return;
    }

    if (sqlConfigured)
    {
        await databaseBootstrapService.EnsureInitializedIfNeededAsync(context.RequestAborted);
    }

    if (sqlConfigured && !licenseStatus.IsActive && !isSqlConnectionPath && !isLicensePath && !isStaticPath)
    {
        context.Response.Redirect("/License/Status");
        return;
    }

    if (sqlConfigured && licenseStatus.IsActive && !isLoggedIn && !isSqlConnectionPath && !isLoginPath && !isLogoutPath && !isStaticPath)
    {
        context.Response.Redirect("/Account/Login");
        return;
    }

    if (sqlConfigured && licenseStatus.IsActive && !isLoggedIn && isLicensePath)
    {
        context.Response.Redirect("/Account/Login");
        return;
    }

    if (sqlConfigured && isLoggedIn && licenseStatus.IsActive && (isSqlConnectionPath || isLicensePath))
    {
        context.Response.Redirect("/Home/Index");
        return;
    }

    if (sqlConfigured && isLoggedIn && licenseStatus.IsActive && !isStaticPath)
    {
        var username = context.User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Response.Redirect("/Account/Login");
            return;
        }

        var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
        var appUser = await dbContext.AppUsers
            .AsNoTracking()
            .Where(x => x.IsActive == true && (x.Code == username || x.NameSurname == username))
            .Select(x => new { x.IsRight })
            .FirstOrDefaultAsync(context.RequestAborted);

        if (appUser is null)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Response.Redirect("/Account/Login");
            return;
        }

        hasRight = appUser.IsRight == true;
        context.Items["HasRight"] = hasRight;
    }

    if (sqlConfigured && isLoggedIn && licenseStatus.IsActive && !hasRight && !isLogoutPath && !isStaticPath)
    {
        var isHomePath = path.StartsWith("/Home", StringComparison.OrdinalIgnoreCase);
        var isChangePasswordPath = path.StartsWith("/Account/ChangePassword", StringComparison.OrdinalIgnoreCase);
        var isAppLogsPath = path.StartsWith("/AppLogs", StringComparison.OrdinalIgnoreCase);
        if (!isHomePath && !isChangePasswordPath && !isAppLogsPath)
        {
            context.Response.Redirect("/Home/Index");
            return;
        }
    }

    await next();
});

app.UseMiddleware<RequestAuditLoggingMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
