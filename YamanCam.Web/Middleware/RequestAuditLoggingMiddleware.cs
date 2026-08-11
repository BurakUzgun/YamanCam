using System.Diagnostics;
using YamanCam.Web.Services;

namespace YamanCam.Web.Middleware;

public class RequestAuditLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestAuditLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAppLogService appLogService, ISqlConnectionSettingsService sqlSettingsService)
    {
        if (!sqlSettingsService.HasValidConfiguration())
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        var path = context.Request.Path.Value ?? string.Empty;
        var isStatic = path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase);

        if (isStatic)
        {
            await _next(context);
            return;
        }

        try
        {
            await _next(context);
            sw.Stop();
            await appLogService.WriteInfoAsync(
                "HttpRequest",
                "RequestCompleted",
                $"{context.Request.Method} {path}",
                statusCode: context.Response.StatusCode,
                durationMs: sw.ElapsedMilliseconds,
                cancellationToken: context.RequestAborted);
        }
        catch (Exception ex)
        {
            sw.Stop();
            await appLogService.WriteErrorAsync(
                "HttpRequest",
                "UnhandledException",
                $"{context.Request.Method} {path} - {ex.Message}",
                statusCode: 500,
                durationMs: sw.ElapsedMilliseconds,
                cancellationToken: context.RequestAborted);
            throw;
        }
    }
}
