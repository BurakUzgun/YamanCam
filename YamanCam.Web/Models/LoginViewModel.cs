using System.ComponentModel.DataAnnotations;

namespace YamanCam.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Kullanici alani zorunludur.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre alani zorunludur.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public bool RememberPassword { get; set; }
}
