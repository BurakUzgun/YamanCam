using YamanCam.Web.Data;
using YamanCam.Web.Infrastructure;
using YamanCam.Web.Middleware;
using YamanCam.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:1926");

builder.Services.AddControllersWithViews(options =>
{
    // decimal/double/float alanları her zaman "." ondalık ile (InvariantCulture) çözülsün.
    // <input type="number"> değeri tarayıcı dilinden bağımsız olarak nokta ile gönderir;
    // sunucu tr-TR kültürüyle çözünce "10.0000" -> 100000 gibi hatalı sonuç oluşuyordu.
    options.ModelBinderProviders.Insert(0, new InvariantNumericModelBinderProvider());
});
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
builder.Services.AddScoped<IUserRightService, UserRightService>();
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
        var appUserExists = await dbContext.AppUsers
            .AsNoTracking()
            .AnyAsync(x => x.IsActive == true && (x.Code == username || x.NameSurname == username), context.RequestAborted);

        if (!appUserExists)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Response.Redirect("/Account/Login");
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
