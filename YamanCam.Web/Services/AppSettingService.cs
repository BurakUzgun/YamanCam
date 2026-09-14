using YamanCam.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace YamanCam.Web.Services;

/// <summary>
/// App_Setting tablosundaki "Küsürat" grubu ayarlarini (ekranlardaki ondalik basamak
/// sayilari) okuyup process genelinde onbelleklemek icin kullanilir. Ayarlar ekrani
/// (AppSettingsController) her Create/Edit/Delete sonrasi InvalidateCache cagirir.
/// </summary>
public class AppSettingService : IAppSettingService
{
    private const string KusuratGroup = "Küsürat";

    private static readonly SemaphoreSlim CacheLock = new(1, 1);
    private static Dictionary<string, int>? _cache;

    private readonly ApplicationDbContext _context;

    public AppSettingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void InvalidateCache()
    {
        _cache = null;
    }

    public async Task<IReadOnlyDictionary<string, int>> GetKusuratMapAsync(CancellationToken cancellationToken = default)
    {
        if (_cache is not null)
        {
            return _cache;
        }

        await CacheLock.WaitAsync(cancellationToken);
        try
        {
            if (_cache is not null)
            {
                return _cache;
            }

            var rows = await _context.AppSettings
                .AsNoTracking()
                .Where(x => x.SettingGroup == KusuratGroup && x.IsActive != false)
                .ToListAsync(cancellationToken);

            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                if (int.TryParse(row.Value?.Trim(), out var decimals))
                {
                    map[row.Explanation.Trim()] = decimals;
                }
            }

            _cache = map;
            return _cache;
        }
        finally
        {
            CacheLock.Release();
        }
    }

    public async Task<int> GetDecimalsAsync(string explanation, int defaultValue = 2, CancellationToken cancellationToken = default)
    {
        var map = await GetKusuratMapAsync(cancellationToken);
        return map.TryGetValue(explanation, out var value) ? value : defaultValue;
    }
}
