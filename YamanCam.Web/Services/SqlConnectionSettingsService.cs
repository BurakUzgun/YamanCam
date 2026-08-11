using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using YamanCam.Web.Models;
using Microsoft.Data.SqlClient;

namespace YamanCam.Web.Services;

public class SqlConnectionSettingsService : ISqlConnectionSettingsService
{
    private const string EncryptionKey = "Bg1711..Aa";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _settingsPath;

    public SqlConnectionSettingsService(IWebHostEnvironment environment)
    {
        _settingsPath = Path.Combine(environment.ContentRootPath, "SQLConnection.json");
    }

    public SqlConnectionSettings GetSettings()
    {
        EnsureFileExists();
        var json = File.ReadAllText(_settingsPath);
        var settings = JsonSerializer.Deserialize<SqlConnectionSettings>(json) ?? new SqlConnectionSettings();

        if (!string.IsNullOrWhiteSpace(settings.Password) && settings.PasswordEncrypted)
        {
            settings.Password = Decrypt(settings.Password);
            settings.PasswordEncrypted = false;
        }

        return settings;
    }

    public async Task SaveSettingsAsync(SqlConnectionSettings settings)
    {
        var storedSettings = new SqlConnectionSettings
        {
            Server = settings.Server,
            Database = settings.Database,
            UseTrustedConnection = settings.UseTrustedConnection,
            UserId = settings.UserId,
            Password = settings.Password,
            TrustServerCertificate = settings.TrustServerCertificate,
            PasswordEncrypted = false
        };

        if (!storedSettings.UseTrustedConnection && !string.IsNullOrWhiteSpace(storedSettings.Password))
        {
            storedSettings.Password = Encrypt(storedSettings.Password);
            storedSettings.PasswordEncrypted = true;
        }

        var json = JsonSerializer.Serialize(storedSettings, JsonOptions);
        await File.WriteAllTextAsync(_settingsPath, json);
    }

    public string BuildConnectionString()
    {
        var settings = GetSettings();
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = settings.Server,
            InitialCatalog = settings.Database,
            TrustServerCertificate = settings.TrustServerCertificate,
            Encrypt = false
        };

        if (settings.UseTrustedConnection)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = settings.UserId;
            builder.Password = settings.Password;
            builder.IntegratedSecurity = false;
        }

        return builder.ConnectionString;
    }

    public bool HasValidConfiguration()
    {
        var settings = GetSettings();
        if (string.IsNullOrWhiteSpace(settings.Server) || string.IsNullOrWhiteSpace(settings.Database))
        {
            return false;
        }

        if (settings.UseTrustedConnection)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(settings.UserId) && !string.IsNullOrWhiteSpace(settings.Password);
    }

    private void EnsureFileExists()
    {
        if (File.Exists(_settingsPath))
        {
            return;
        }

        var defaultSettings = new SqlConnectionSettings();
        var json = JsonSerializer.Serialize(defaultSettings, JsonOptions);
        File.WriteAllText(_settingsPath, json);
    }

    private static string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = GetKeyBytes();
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return $"{Convert.ToBase64String(aes.IV)}:{Convert.ToBase64String(encryptedBytes)}";
    }

    private static string Decrypt(string encryptedText)
    {
        var parts = encryptedText.Split(':');
        if (parts.Length != 2)
        {
            return encryptedText;
        }

        using var aes = Aes.Create();
        aes.Key = GetKeyBytes();
        aes.IV = Convert.FromBase64String(parts[0]);

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var cipherBytes = Convert.FromBase64String(parts[1]);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    private static byte[] GetKeyBytes()
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(EncryptionKey));
    }
}
