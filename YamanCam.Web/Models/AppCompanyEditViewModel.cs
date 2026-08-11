using System.ComponentModel.DataAnnotations;

namespace YamanCam.Web.Models;

public class AppCompanyEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Şirket kodu zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Şirket Kodu")]
    public string CompanyCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şirket adı zorunludur.")]
    [StringLength(200)]
    [Display(Name = "Şirket Adı")]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "Vergi No")]
    public string? TaxNumber { get; set; }

    [StringLength(100)]
    [Display(Name = "Vergi Dairesi")]
    public string? TaxOffice { get; set; }

    [StringLength(500)]
    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [StringLength(100)]
    [Display(Name = "Şehir")]
    public string? City { get; set; }

    [StringLength(30)]
    [Display(Name = "Telefon")]
    public string? Phone { get; set; }

    [StringLength(150)]
    [Display(Name = "E-posta")]
    public string? Email { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
