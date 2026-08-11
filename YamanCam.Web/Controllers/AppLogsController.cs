using YamanCam.Web.Data;
using YamanCam.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace YamanCam.Web.Controllers;

public class AppLogsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AppLogsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? fromUtc, DateTime? toUtc, string? level, string? logType, string? userName, string? searchText)
    {
        var query = BuildFilteredQuery(fromUtc, toUtc, level, logType, userName, searchText);

        var logs = await query
            .AsNoTracking()
            .OrderByDescending(x => x.LogDateUtc)
            .Take(500)
            .ToListAsync();

        var vm = new AppLogListViewModel
        {
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Level = level,
            LogType = logType,
            UserName = userName,
            SearchText = searchText,
            Logs = logs
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(DateTime? fromUtc, DateTime? toUtc, string? level, string? logType, string? userName, string? searchText)
    {
        var logs = await BuildFilteredQuery(fromUtc, toUtc, level, logType, userName, searchText)
            .AsNoTracking()
            .OrderByDescending(x => x.LogDateUtc)
            .Take(5000)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("LogDateUtc,Level,Category,EventName,UserName,UserId,IpAddress,Method,Path,QueryString,StatusCode,DurationMs,OldValue,NewValue,Message");

        foreach (var log in logs)
        {
            var columns = new[]
            {
                log.LogDateUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                log.Level,
                log.Category ?? string.Empty,
                log.EventName ?? string.Empty,
                log.UserName ?? string.Empty,
                log.UserId?.ToString() ?? string.Empty,
                log.IpAddress ?? string.Empty,
                log.Method ?? string.Empty,
                log.Path ?? string.Empty,
                log.QueryString ?? string.Empty,
                log.StatusCode?.ToString() ?? string.Empty,
                log.DurationMs?.ToString() ?? string.Empty,
                log.OldValue ?? string.Empty,
                log.NewValue ?? string.Empty,
                log.Message ?? string.Empty
            };

            sb.AppendLine(string.Join(",", columns.Select(EscapeCsv)));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var logTypePart = string.IsNullOrWhiteSpace(logType) ? "all" : logType.Trim().ToLowerInvariant();
        var fileName = $"app-logs-{logTypePart}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    private IQueryable<AppLog> BuildFilteredQuery(DateTime? fromUtc, DateTime? toUtc, string? level, string? logType, string? userName, string? searchText)
    {
        var query = _context.AppLogs.AsQueryable();

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.LogDateUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.LogDateUtc <= toUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(level))
        {
            query = query.Where(x => x.Level == level);
        }

        if (!string.IsNullOrWhiteSpace(logType))
        {
            var normalizedLogType = logType.Trim().ToLowerInvariant();
            if (normalizedLogType == "system")
            {
                query = query.Where(x => x.Category == "HttpRequest");
            }
            else if (normalizedLogType == "application")
            {
                query = query.Where(x => x.Category != "HttpRequest");
            }
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            query = query.Where(x => x.UserName != null && x.UserName.Contains(userName));
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(x =>
                (x.Category != null && x.Category.Contains(searchText))
                || (x.EventName != null && x.EventName.Contains(searchText))
                || x.Message.Contains(searchText)
                || (x.Path != null && x.Path.Contains(searchText))
                || (x.OldValue != null && x.OldValue.Contains(searchText))
                || (x.NewValue != null && x.NewValue.Contains(searchText)));
        }

        return query;
    }

    private static string EscapeCsv(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
