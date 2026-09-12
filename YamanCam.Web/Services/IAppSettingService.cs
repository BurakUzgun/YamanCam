namespace YamanCam.Web.Services;

public interface IAppSettingService
{
    Task<IReadOnlyDictionary<string, int>> GetKusuratMapAsync(CancellationToken cancellationToken = default);

    Task<int> GetDecimalsAsync(string explanation, int defaultValue = 2, CancellationToken cancellationToken = default);

    void InvalidateCache();
}
