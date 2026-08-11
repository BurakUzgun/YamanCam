using YamanCam.Web.Models;

namespace YamanCam.Web.Services;

public interface ICenterLicenseConnectionService
{
    void EnsureConnectionFileExists();
    LicenseStatusSnapshot GetCurrentStatus();
    Task<LicenseStatusSnapshot> RefreshStatusIfNeededAsync(CancellationToken cancellationToken = default);
    Task<LicenseStatusSnapshot> RefreshStatusNowAsync(CancellationToken cancellationToken = default);
    Task<LicenseRequestResult> CreateLicenseRequestAsync(LicenseRequestData requestData, CancellationToken cancellationToken = default);
}
