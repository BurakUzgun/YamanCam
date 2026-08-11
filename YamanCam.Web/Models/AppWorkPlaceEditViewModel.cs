using System.ComponentModel.DataAnnotations;

namespace YamanCam.Web.Models;

public class AppWorkPlaceEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Şirket seçimi zorunludur.")]
    [Display(Name = "Şirket")]
    public int CompanyId { get; set; }

    [Required(ErrorMessage = "Şube kodu zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Şube Kodu")]
    public string WorkPlaceCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şube adı zorunludur.")]
    [StringLength(200)]
    [Display(Name = "Şube Adı")]
    public string WorkPlaceName { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [StringLength(100)]
    [Display(Name = "Şehir")]
    public string? City { get; set; }

    [StringLength(30)]
    [Display(Name = "Telefon")]
    public string? Phone { get; set; }

    [Display(Name = "Varsayılan Şube")]
    public bool IsDefault { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public List<CompanyOptionViewModel> CompanyOptions { get; set; } = [];
}

public class CompanyOptionViewModel
{
    public int CompanyId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

public class AppWorkPlaceListItemViewModel
{
    public int RecId { get; set; }
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string? WorkPlaceCode { get; set; }
    public string? WorkPlaceName { get; set; }
    public string? City { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
}

public class AppWorkPlaceListViewModel
{
    public int? CompanyId { get; set; }
    public List<CompanyOptionViewModel> CompanyOptions { get; set; } = [];
    public List<AppWorkPlaceListItemViewModel> Items { get; set; } = [];
}
