using YamanCam.Web.Data;
using YamanCam.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Services;

public class AppLogService : IAppLogService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AppLogService> _logger;

    public AppLogService(
        ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AppLogService> logger)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Task WriteInfoAsync(
        string category,
        string eventName,
        string message,
        int? statusCode = null,
        long? durationMs = null,
        string? oldValue = null,
        string? newValue = null,
        CancellationToken cancellationToken = default)
    {
        return WriteInternalAsync("Info", category, eventName, message, oldValue, newValue, statusCode, durationMs, cancellationToken);
    }

    public Task WriteErrorAsync(
        string category,
        string eventName,
        string message,
        int? statusCode = null,
        long? durationMs = null,
        string? oldValue = null,
        string? newValue = null,
        CancellationToken cancellationToken = default)
    {
        return WriteInternalAsync("Error", category, eventName, message, oldValue, newValue, statusCode, durationMs, cancellationToken);
    }

    private async Task WriteInternalAsync(
        string level,
        string category,
        string eventName,
        string message,
        string? oldValue,
        string? newValue,
        int? statusCode,
        long? durationMs,
        CancellationToken cancellationToken)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            var userName = context?.User?.Identity?.Name;
            int? userId = null;
            if (int.TryParse(context?.User?.FindFirst("UserId")?.Value, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var log = new AppLog
            {
                LogDateUtc = DateTime.UtcNow,
                Level = level,
                Category = category,
                EventName = eventName,
                Message = message,
                OldValue = oldValue,
                NewValue = newValue,
                UserName = userName,
                UserId = userId,
                IpAddress = context?.Connection.RemoteIpAddress?.ToString(),
                Method = context?.Request.Method,
                Path = context?.Request.Path.Value,
                QueryString = context?.Request.QueryString.Value,
                StatusCode = statusCode,
                DurationMs = durationMs
            };

            _dbContext.AppLogs.Add(log);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "App_Log kaydi yazilamadi. Category={Category}, Event={EventName}", category, eventName);
        }
    }
}
