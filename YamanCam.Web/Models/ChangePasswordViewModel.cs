using System.ComponentModel.DataAnnotations;

namespace YamanCam.Web.Models;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Kullanici kodu zorunludur.")]
    [Display(Name = "Kullanici Kodu")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mevcut sifre zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mevcut Sifre")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni sifre zorunludur.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Yeni sifre en az 1 karakter olmalidir.")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Sifre")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni sifre tekrar zorunludur.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Yeni sifreler ayni degil.")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Sifre Tekrar")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
