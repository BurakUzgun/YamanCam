using YamanCam.Web.Models;

namespace YamanCam.Web.Services;

public interface ISqlConnectionSettingsService
{
    SqlConnectionSettings GetSettings();
    Task SaveSettingsAsync(SqlConnectionSettings settings);
    string BuildConnectionString();
    bool HasValidConfiguration();
}
