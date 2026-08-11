namespace YamanCam.Web.Models;

public class SqlConnectionSettings
{
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public bool UseTrustedConnection { get; set; } = true;
    public string? UserId { get; set; }
    public string? Password { get; set; }
    public bool PasswordEncrypted { get; set; }
    public bool TrustServerCertificate { get; set; } = true;
}
