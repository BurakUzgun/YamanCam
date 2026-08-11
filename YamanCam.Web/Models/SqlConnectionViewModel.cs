using System.ComponentModel.DataAnnotations;

namespace YamanCam.Web.Models;

public class SqlConnectionViewModel
{
    [Required(ErrorMessage = "Server alani zorunludur.")]
    public string Server { get; set; } = string.Empty;

    [Required(ErrorMessage = "Database alani zorunludur.")]
    public string Database { get; set; } = string.Empty;

    [Display(Name = "Windows Authentication")]
    public bool UseTrustedConnection { get; set; } = true;

    [Display(Name = "User Id")]
    public string? UserId { get; set; }

    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Display(Name = "Trust Server Certificate")]
    public bool TrustServerCertificate { get; set; } = true;
}
