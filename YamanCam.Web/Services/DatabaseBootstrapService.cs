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
