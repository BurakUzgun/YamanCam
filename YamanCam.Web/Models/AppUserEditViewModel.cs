using System.ComponentModel.DataAnnotations;

namespace YamanCam.Web.Models;

public class AppUserEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Kullanıcı kodu zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Kullanıcı Kodu")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Ad Soyad")]
    public string NameSurname { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Parola")]
    public string? Password { get; set; }

    [StringLength(150)]
    [Display(Name = "E-posta")]
    public string? Email { get; set; }

    [StringLength(30)]
    [Display(Name = "Telefon")]
    public string? Phone { get; set; }

    [Display(Name = "Yönetici Yetkisi")]
    public bool IsRight { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Varsayılan Şube")]
    public int? DefaultWorkPlaceId { get; set; }

    public List<int> SelectedWorkPlaceIds { get; set; } = [];

    public List<UserWorkPlaceOptionViewModel> WorkPlaceOptions { get; set; } = [];

    public List<ScreenRightCatalogItem> ScreenCatalog { get; set; } = [];

    public List<string> SelectedScreenRightKeys { get; set; } = [];
}

public class UserWorkPlaceOptionViewModel
{
    public int WorkPlaceId { get; set; }
    public string? WorkPlaceCode { get; set; }
    public string? WorkPlaceName { get; set; }
    public bool IsSelected { get; set; }
}

public class ScreenRightCatalogItem
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool HasDelete { get; set; } = true;
}
