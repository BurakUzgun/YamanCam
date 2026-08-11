using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using YamanCam.Web.Models;
using Microsoft.Data.SqlClient;

namespace YamanCam.Web.Services;

public class CenterLicenseConnectionService : ICenterLicenseConnectionService
{
    private const string ModuleName = "CntMaliyet";
    private const string EncryptionKey = "Bg1711..Aa";
    private const string DefaultPassword = "..Center3540**";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _settingsPath;
    private readonly string _statePath;
    private readonly ILogger<CenterLicenseConnectionService> _logger;
    private readonly SemaphoreSlim _checkLock = new(1, 1);
    private LicenseStatusSnapshot _lastSnapshot = new()
    {
        Message = "Lisans kontrolu henuz yapilmadi."
    };

    public CenterLicenseConnectionService(IWebHostEnvironment environment, ILogger<CenterLicenseConnectionService> logger)
    {
        _settingsPath = Path.Combine(environment.ContentRootPath, "CenterSQLConnection.json");
        _statePath = Path.Combine(environment.ContentRootPath, "LicenseState.json");
        _logger = logger;
    }

    public void EnsureConnectionFileExists()
    {
        if (File.Exists(_settingsPath))
        {
            return;
        }

        var settings = new CenterSqlConnectionSettings
        {
            Password = Encrypt(DefaultPassword),
            PasswordEncrypted = true
        };

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_settingsPath, json);
    }

    public LicenseStatusSnapshot GetCurrentStatus()
    {
        return _lastSnapshot;
    }

    public async Task<LicenseStatusSnapshot> RefreshStatusIfNeededAsync(CancellationToken cancellationToken = default)
    {
        return await RefreshInternalAsync(force: false, cancellationToken);
    }

    public async Task<LicenseStatusSnapshot> RefreshStatusNowAsync(CancellationToken cancellationToken = default)
    {
        return await RefreshInternalAsync(force: true, cancellationToken);
    }

    private async Task<LicenseStatusSnapshot> RefreshInternalAsync(bool force, CancellationToken cancellationToken)
    {
        EnsureConnectionFileExists();
        EnsureStateFileExists();
        var state = GetState();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (!force && state.LastCheckedUtc.HasValue && DateOnly.FromDateTime(state.LastCheckedUtc.Value) == today)
        {
            _lastSnapshot = BuildSnapshotFromState(state);
            return _lastSnapshot;
        }

        await _checkLock.WaitAsync(cancellationToken);
        try
        {
            state = GetState();
            if (!force && state.LastCheckedUtc.HasValue && DateOnly.FromDateTime(state.LastCheckedUtc.Value) == today)
            {
                _lastSnapshot = BuildSnapshotFromState(state);
                return _lastSnapshot;
            }

            if (!state.RequestId.HasValue && string.IsNullOrWhiteSpace(state.HotelName))
            {
                state.LastCheckedUtc = DateTime.UtcNow;
                state.IsActive = false;
                state.LastMessage = "Lisans talebi bulunamadi.";
                SaveState(state);
                _lastSnapshot = BuildSnapshotFromState(state);
                return _lastSnapshot;
            }

            await using var connection = new SqlConnection(BuildConnectionString());
            await connection.OpenAsync(cancellationToken);
            var idColumn = await ResolveIdColumnAsync(connection, cancellationToken);

            string sql;
            await using var command = new SqlCommand();
            command.Connection = connection;
            if (state.RequestId.HasValue && !string.IsNullOrWhiteSpace(idColumn))
            {
                sql = $"""
                    SELECT TOP 1 IsActive, EndOfDate
                    FROM dbo.Erp_Licence
                    WHERE [{idColumn}] = @RequestId AND Module = @Module
                    """;
                command.Parameters.AddWithValue("@RequestId", state.RequestId.Value);
            }
            else
            {
                sql = """
                    SELECT TOP 1 IsActive, EndOfDate
                    FROM dbo.Erp_Licence
                    WHERE Module = @Module
                      AND HotelName = @HotelName
                      AND CompanyName = @CompanyName
                      AND LicenceUser = @LicenceUser
                    ORDER BY SalesDate DESC
                    """;
                command.Parameters.AddWithValue("@HotelName", state.HotelName ?? string.Empty);
                command.Parameters.AddWithValue("@CompanyName", state.CompanyName ?? string.Empty);
                command.Parameters.AddWithValue("@LicenceUser", state.LicenceUser ?? string.Empty);
            }

            command.CommandText = sql;
            command.Parameters.AddWithValue("@Module", ModuleName);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var isActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]);
                var endOfDate = reader["EndOfDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EndOfDate"]);
                var isExpired = endOfDate.HasValue && endOfDate.Value.Date < DateTime.UtcNow.Date;

                state.LastCheckedUtc = DateTime.UtcNow;
                state.IsActive = isActive && !isExpired;
                state.EndOfDate = endOfDate;
                state.LastMessage = state.IsActive ? "Lisans aktif." : "Lisans beklemede veya pasif.";
            }
            else
            {
                state.LastCheckedUtc = DateTime.UtcNow;
                state.IsActive = false;
                state.LastMessage = "Lisans kaydi merkezde bulunamadi.";
            }

            SaveState(state);
            _lastSnapshot = BuildSnapshotFromState(state);
            _logger.LogInformation("Daily license check completed. Active: {IsActive}", _lastSnapshot.IsActive);
            return _lastSnapshot;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Daily license connection check failed.");
            var fallback = GetState();
            fallback.LastMessage = "Merkez lisans kontrolune ulasilamadi.";
            _lastSnapshot = BuildSnapshotFromState(fallback);
            return _lastSnapshot;
        }
        finally
        {
            _checkLock.Release();
        }
    }

    public async Task<LicenseRequestResult> CreateLicenseRequestAsync(LicenseRequestData requestData, CancellationToken cancellationToken = default)
    {
        EnsureConnectionFileExists();
        EnsureStateFileExists();
        try
        {
            var salesDate = DateTime.UtcNow;
            var endOfDate = salesDate.AddYears(1);
            var customerIdCode = GenerateCustomerIdCode(requestData);

            const string insertSql = """
                INSERT INTO dbo.Erp_Licence
                (HotelName, CompanyName, RoomCount, SalesDate, EndOfDate, LicenceUser, ErpName, SalesConsultant, IsActive, Module, CustomerId)
                VALUES
                (@HotelName, @CompanyName, @RoomCount, @SalesDate, @EndOfDate, @LicenceUser, @ErpName, @SalesConsultant, @IsActive, @Module, @CustomerId)
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """;

            await using var connection = new SqlConnection(BuildConnectionString());
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(insertSql, connection);
            command.Parameters.AddWithValue("@HotelName", requestData.HotelName);
            command.Parameters.AddWithValue("@CompanyName", requestData.CompanyName);
            command.Parameters.AddWithValue("@RoomCount", requestData.RoomCount);
            command.Parameters.AddWithValue("@SalesDate", salesDate);
            command.Parameters.AddWithValue("@EndOfDate", endOfDate);
            command.Parameters.AddWithValue("@LicenceUser", requestData.LicenceUser);
            command.Parameters.AddWithValue("@ErpName", string.Empty);
            command.Parameters.AddWithValue("@SalesConsultant", string.Empty);
            command.Parameters.AddWithValue("@IsActive", false);
            command.Parameters.AddWithValue("@Module", ModuleName);
            command.Parameters.AddWithValue("@CustomerId", customerIdCode);

            var insertedId = await command.ExecuteScalarAsync(cancellationToken);
            int? requestId = null;
            if (insertedId is not null && insertedId != DBNull.Value)
            {
                requestId = Convert.ToInt32(insertedId);
            }

            var state = new LicenseStateData
            {
                RequestId = requestId,
                HotelName = requestData.HotelName,
                CompanyName = requestData.CompanyName,
                LicenceUser = requestData.LicenceUser,
                IsActive = false,
                EndOfDate = endOfDate,
                LastCheckedUtc = DateTime.UtcNow,
                LastMessage = "Lisans talebi gonderildi, onay bekleniyor."
            };

            SaveState(state);
            _lastSnapshot = BuildSnapshotFromState(state);

            return new LicenseRequestResult
            {
                Success = true,
                RequestId = requestId,
                Message = $"Lisans talebi gonderildi. IsActive alani merkezde false olarak acildi. CustomerId: {customerIdCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "License request creation failed.");
            return new LicenseRequestResult
            {
                Success = false,
                Message = $"Lisans talebi gonderilemedi: {ex.Message}"
            };
        }
    }

    private string BuildConnectionString()
    {
        var settings = GetSettings();
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = settings.Server,
            InitialCatalog = settings.Database,
            UserID = settings.UserId,
            Password = settings.Password,
            IntegratedSecurity = false,
            TrustServerCertificate = settings.TrustServerCertificate,
            Encrypt = false
        };

        return builder.ConnectionString;
    }

    private CenterSqlConnectionSettings GetSettings()
    {
        EnsureConnectionFileExists();
        var json = File.ReadAllText(_settingsPath);
        var settings = JsonSerializer.Deserialize<CenterSqlConnectionSettings>(json) ?? new CenterSqlConnectionSettings();

        if (!string.IsNullOrWhiteSpace(settings.Password) && settings.PasswordEncrypted)
        {
            settings.Password = Decrypt(settings.Password);
        }

        return settings;
    }

    private void EnsureStateFileExists()
    {
        if (File.Exists(_statePath))
        {
            return;
        }

        var state = new LicenseStateData();
        File.WriteAllText(_statePath, JsonSerializer.Serialize(state, JsonOptions));
    }

    private LicenseStateData GetState()
    {
        EnsureStateFileExists();
        var json = File.ReadAllText(_statePath);
        return JsonSerializer.Deserialize<LicenseStateData>(json) ?? new LicenseStateData();
    }

    private void SaveState(LicenseStateData state)
    {
        var json = JsonSerializer.Serialize(state, JsonOptions);
        File.WriteAllText(_statePath, json);
    }

    private static LicenseStatusSnapshot BuildSnapshotFromState(LicenseStateData state)
    {
        var isExpired = state.EndOfDate.HasValue && state.EndOfDate.Value.Date < DateTime.UtcNow.Date;
        return new LicenseStatusSnapshot
        {
            HasRequest = state.RequestId.HasValue,
            RequestId = state.RequestId,
            IsActive = state.IsActive && !isExpired,
            IsExpired = isExpired,
            EndOfDate = state.EndOfDate,
            LastCheckedUtc = state.LastCheckedUtc,
            Message = state.LastMessage ?? string.Empty
        };
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

    private static int GenerateCustomerIdCode(LicenseRequestData requestData)
    {
        var raw = $"{Environment.MachineName}|{requestData.HotelName}|{requestData.CompanyName}|CntMaliyet";
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
        var value = BitConverter.ToInt32(hash, 0) & 0x7FFFFFFF;
        return value == 0 ? 1 : value;
    }

    private static async Task<string?> ResolveIdColumnAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string columnSql = """
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Erp_Licence'
            """;

        await using var cmd = new SqlCommand(columnSql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(reader.GetString(0));
        }

        var candidates = new[] { "BgId", "Id", "LicenseId", "LicenceId" };
        return candidates.FirstOrDefault(candidate => columns.Contains(candidate));
    }

    private sealed class LicenseStateData
    {
        public int? RequestId { get; set; }
        public string? HotelName { get; set; }
        public string? CompanyName { get; set; }
        public string? LicenceUser { get; set; }
        public bool IsActive { get; set; }
        public DateTime? EndOfDate { get; set; }
        public DateTime? LastCheckedUtc { get; set; }
        public string? LastMessage { get; set; }
    }
}
