namespace YamanCam.Web.Models;

public class CenterSqlConnectionSettings
{
    public string Server { get; set; } = "77.245.150.215,11444";
    public string Database { get; set; } = "Center_Licance";
    public string UserId { get; set; } = "sa";
    public string Password { get; set; } = string.Empty;
    public bool PasswordEncrypted { get; set; }
    public bool TrustServerCertificate { get; set; } = true;
}
