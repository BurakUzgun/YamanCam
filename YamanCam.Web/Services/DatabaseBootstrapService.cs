using System.Text;
using Microsoft.Data.SqlClient;

namespace YamanCam.Web.Services;

public class DatabaseBootstrapService : IDatabaseBootstrapService
{
    private const string DefaultUserCode = "Center";
    private const string DefaultUserPassword = "1234";

    private readonly ISqlConnectionSettingsService _sqlSettingsService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DatabaseBootstrapService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _initialized;

    public DatabaseBootstrapService(
        ISqlConnectionSettingsService sqlSettingsService,
        IWebHostEnvironment environment,
        ILogger<DatabaseBootstrapService> logger)
    {
        _sqlSettingsService = sqlSettingsService;
        _environment = environment;
        _logger = logger;
    }

    public async Task EnsureInitializedIfNeededAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized || !_sqlSettingsService.HasValidConfiguration())
        {
            return;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized || !_sqlSettingsService.HasValidConfiguration())
            {
                return;
            }

            await using var connection = new SqlConnection(_sqlSettingsService.BuildConnectionString());
            await connection.OpenAsync(cancellationToken);

            var scriptPath = Path.Combine(_environment.ContentRootPath, "Scripts", "initial-schema.sql");
            if (!File.Exists(scriptPath))
            {
                _logger.LogWarning("Schema script was not found: {Path}", scriptPath);
                return;
            }

            var script = await File.ReadAllTextAsync(scriptPath, cancellationToken);
            var batches = SplitSqlBatches(script);
            foreach (var batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch))
                {
                    continue;
                }

                await using var command = new SqlCommand(batch, connection);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            var schemaVersion = await GetSchemaVersionAsync(connection, cancellationToken);
            _logger.LogInformation("Database schema script executed. YamanCam.Core version: {Version}", schemaVersion);

            await TrySeedDefaultUserAsync(connection, cancellationToken);
            await TrySeedDefaultSettingsAsync(connection, cancellationToken);

            _initialized = true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task TrySeedDefaultUserAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string tableExistsSql = "SELECT OBJECT_ID(N'dbo.App_User', N'U');";

        await using (var cmd = new SqlCommand(tableExistsSql, connection))
        {
            var objId = await cmd.ExecuteScalarAsync(cancellationToken);
            if (objId is null || objId == DBNull.Value || Convert.ToInt32(objId) == 0)
            {
                return;
            }
        }

        const string countSql = "SELECT COUNT(*) FROM dbo.App_User;";
        await using (var countCmd = new SqlCommand(countSql, connection))
        {
            var count = Convert.ToInt32(await countCmd.ExecuteScalarAsync(cancellationToken));
            if (count > 0)
            {
                return;
            }
        }

        const string insertSql = """
            INSERT INTO dbo.App_User ([Code], [NameSurname], [Password], [IsRight], [IsActive])
            VALUES (@Code, @NameSurname, @Password, @IsRight, @IsActive);
            """;

        await using var insertCmd = new SqlCommand(insertSql, connection);
        insertCmd.Parameters.AddWithValue("@Code", DefaultUserCode);
        insertCmd.Parameters.AddWithValue("@NameSurname", DefaultUserCode);
        insertCmd.Parameters.AddWithValue("@Password", DefaultUserPassword);
        insertCmd.Parameters.AddWithValue("@IsRight", true);
        insertCmd.Parameters.AddWithValue("@IsActive", true);

        await insertCmd.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation(
            "Varsayilan kullanici eklendi (App_User bosken): Code={Code}, NameSurname={Name}.",
            DefaultUserCode,
            DefaultUserCode);
    }

    private static readonly (string SettingGroup, string Explanation, string Value)[] DefaultSettings =
    [
        ("Küsürat", "Alış Faturaları Kur", "2"),
        ("Küsürat", "Alış Faturaları Toplam (Döviz)", "2"),
        ("Küsürat", "Alış Faturaları Toplam (TL)", "2"),
        ("Küsürat", "Yeni Alış Faturası Miktar", "2"),
        ("Küsürat", "Yeni Alış Faturası Birim Fiyat", "2"),
        ("Küsürat", "Yeni Alış Faturası KDV %", "2"),
        ("Küsürat", "Yeni Alış Faturası Tevkifat", "2"),
        ("Küsürat", "Yeni Alış Faturası Tev. %", "2"),
        ("Küsürat", "Yeni Alış Faturası Net (Döviz)", "2"),
        ("Küsürat", "Yeni Alış Faturası KDV (Döviz)", "2"),
        ("Küsürat", "Yeni Alış Faturası Tevkifat", "2"),
        ("Küsürat", "Yeni Alış Faturası Net KDV", "2"),
        ("Küsürat", "Yeni Alış Faturası Toplam (Döviz)", "2"),
        ("Küsürat", "Yeni Alış Faturası Mal Bedeli", "2"),
        ("Küsürat", "Yeni Alış Faturası Genel Toplam", "2"),
        ("Küsürat", "Satış Faturaları Kur", "2"),
        ("Küsürat", "Satış Faturaları Toplam (Döviz)", "2"),
        ("Küsürat", "Satış Faturaları Toplam (TL)", "2"),
        ("Küsürat", "Yeni Satış Faturası Miktar", "2"),
        ("Küsürat", "Yeni Satış Faturası Birim Fiyat", "2"),
        ("Küsürat", "Yeni Satış Faturası KDV %", "2"),
        ("Küsürat", "Yeni Satış Faturası Tevkifat", "2"),
        ("Küsürat", "Yeni Satış Faturası Tev. %", "2"),
        ("Küsürat", "Yeni Satış Faturası Net (Döviz)", "2"),
        ("Küsürat", "Yeni Satış Faturası KDV (Döviz)", "2"),
        ("Küsürat", "Yeni Satış Faturası Tevkifat", "2"),
        ("Küsürat", "Yeni Satış Faturası Net KDV", "2"),
        ("Küsürat", "Yeni Satış Faturası Toplam (Döviz)", "2"),
        ("Küsürat", "Yeni Satış Faturası Mal Bedeli", "2"),
        ("Küsürat", "Yeni Satış Faturası Genel Toplam", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası Matrah", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası KDV Tutar", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası Tevkifat", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası Genel Tutar", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası Döviz Tutar", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası Tutar", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası Kdv İnd. Oran", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası Kdv Oran Tevkifat", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası Tev Oran", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası KDV İndirim", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası Net KDV", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası Toplam", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası Tevkifat Tutar", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası Net KDV Tutar", "2"),
        ("Küsürat", "Yeni Gümrük Nakliye Faturası Döviz Kuru", "2"),
        ("Küsürat", "Stok Açılış Fişleri Toplam Tutar", "2"),
        ("Küsürat", "Yeni Stok Açılış Fişi Adet", "2"),
        ("Küsürat", "Yeni Stok Açılış Fişi Birim Fiyat", "2"),
        ("Küsürat", "Yeni Stok Açılış Fişi Toplam Fiyat", "2"),
        ("Küsürat", "Yeni Stok Açılış Fişi Genel Toplam", "2"),
        ("Küsürat", "Şubeler Arası Transfer Fişleri Toplam Tutar", "2"),
        ("Küsürat", "Yeni Şubeler Arası Transfer Fişi Adet", "2"),
        ("Küsürat", "Yeni Şubeler Arası Transfer Fişi Birim Fiyat", "2"),
        ("Küsürat", "Yeni Şubeler Arası Transfer Fişi Toplam Fiyat", "2"),
        ("Küsürat", "Yeni Şubeler Arası Transfer Fişi Genel Toplam", "2"),
        ("Küsürat", "Stok Çıkış Fişleri Toplam Tutar", "2"),
        ("Küsürat", "Yeni Stok Çıkış Fişi Adet", "2"),
        ("Küsürat", "Yeni Stok Çıkış Fişi Birim Fiyat", "2"),
        ("Küsürat", "Yeni Stok Çıkış Fişi Toplam Fiyat", "2"),
        ("Küsürat", "Yeni Stok Çıkış Fişi Genel Toplam", "2"),
        ("Küsürat", "Yeni Stok Ürün Üretim Fişi Miktar", "2"),
        ("Küsürat", "Yeni Stok Ürün Üretim Fişi Hammadde Birim Fiyat", "2"),
        ("Küsürat", "Yeni Stok Ürün Üretim Fişi Hammadde Tutar", "2"),
        ("Küsürat", "Yeni Stok Ürün Üretim Fişi Fire Oran %", "2"),
        ("Küsürat", "Yeni Stok Ürün Üretim Fişi Miktar (Mamül)", "2"),
        ("Küsürat", "Yeni Stok Ürün Üretim Fişi Mamul Birim Fiyat", "2"),
        ("Küsürat", "Yeni Stok Ürün Üretim Fişi Mamul Tutar", "2"),
        ("Küsürat", "Stok Detay Miktar", "4"),
        ("Küsürat", "Stok Detay Birim Fiyat", "4"),
        ("Küsürat", "Stok Detay Net Tutar", "2"),
        ("Küsürat", "Stok Detay Yürüyen Maliyet", "4"),
        ("Küsürat", "Stok Detay Döviz Kuru", "4"),
        ("Küsürat", "Stok Detay Döviz Tutar", "2"),
        ("Küsürat", "Yeni Sayım Kaydı Sayım Miktar", "2"),
        ("Küsürat", "Yeni Farklı Ürün Transfer Fişi Adet", "2"),
        ("Küsürat", "Yeni Farklı Ürün Transfer Fişi Birim Fiyat", "2"),
        ("Küsürat", "Yeni Farklı Ürün Transfer Fişi Toplam Fiyat", "2"),
        ("Küsürat", "Yeni Farklı Ürün Transfer Fişi Genel Toplam", "2"),
        ("Küsürat", "Tediye Fişleri Toplam Borç", "2"),
        ("Küsürat", "Tediye Fişleri Toplam Alacak", "2"),
        ("Küsürat", "Yeni Tediye Fişi Tutar", "2"),
        ("Küsürat", "Yeni Tediye Fişi Toplam", "2"),
        ("Küsürat", "Tahsilat Fişleri Toplam Borç", "2"),
        ("Küsürat", "Tahsilat Fişleri Toplam Alacak", "2"),
        ("Küsürat", "Yeni Tahsilat Fişi Tutar", "2"),
        ("Küsürat", "Yeni Tahsilat Fişi Toplam", "2"),
        ("Küsürat", "Mahsup Fişleri Toplam Borç", "2"),
        ("Küsürat", "Mahsup Fişleri Toplam Alacak", "2"),
        ("Küsürat", "Yeni Mahsup Fişi Toplam", "2"),
    ];

    private async Task TrySeedDefaultSettingsAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string tableExistsSql = "SELECT OBJECT_ID(N'dbo.App_Setting', N'U');";

        await using (var cmd = new SqlCommand(tableExistsSql, connection))
        {
            var objId = await cmd.ExecuteScalarAsync(cancellationToken);
            if (objId is null || objId == DBNull.Value || Convert.ToInt32(objId) == 0)
            {
                return;
            }
        }

        const string countSql = "SELECT COUNT(*) FROM dbo.App_Setting;";
        await using (var countCmd = new SqlCommand(countSql, connection))
        {
            var count = Convert.ToInt32(await countCmd.ExecuteScalarAsync(cancellationToken));
            if (count > 0)
            {
                return;
            }
        }

        const string insertSql = """
            INSERT INTO dbo.App_Setting ([SettingGroup], [Explanation], [Value], [IsActive])
            VALUES (@SettingGroup, @Explanation, @Value, @IsActive);
            """;

        foreach (var setting in DefaultSettings)
        {
            await using var insertCmd = new SqlCommand(insertSql, connection);
            insertCmd.Parameters.AddWithValue("@SettingGroup", setting.SettingGroup);
            insertCmd.Parameters.AddWithValue("@Explanation", setting.Explanation);
            insertCmd.Parameters.AddWithValue("@Value", setting.Value);
            insertCmd.Parameters.AddWithValue("@IsActive", true);
            await insertCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Varsayilan ayarlar eklendi (App_Setting bosken): {Count} kayit.",
            DefaultSettings.Length);
    }

    private static async Task<int> GetSchemaVersionAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string tableExistsSql = "SELECT OBJECT_ID(N'dbo.App_SchemaVersion', N'U');";
        await using (var existsCmd = new SqlCommand(tableExistsSql, connection))
        {
            var objId = await existsCmd.ExecuteScalarAsync(cancellationToken);
            if (objId is null || objId == DBNull.Value || Convert.ToInt32(objId) == 0)
            {
                return 0;
            }
        }

        const string sql = """
            SELECT ISNULL(MAX(VersionNo), 0)
            FROM dbo.App_SchemaVersion
            WHERE ScriptName = N'YamanCam.Core'
            """;

        await using var command = new SqlCommand(sql, connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    private static List<string> SplitSqlBatches(string script)
    {
        var batches = new List<string>();
        var current = new StringBuilder();
        using var reader = new StringReader(script);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.Equals(line.Trim(), "GO", StringComparison.OrdinalIgnoreCase))
            {
                batches.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.AppendLine(line);
        }

        if (current.Length > 0)
        {
            batches.Add(current.ToString());
        }

        return batches;
    }
}
