using System.ComponentModel.DataAnnotations;

namespace YamanCam.Web.Models;

public class AppSettingEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Grup zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Grup")]
    public string SettingGroup { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama zorunludur.")]
    [StringLength(200)]
    [Display(Name = "Açıklama")]
    public string Explanation { get; set; } = string.Empty;

    [Required(ErrorMessage = "Değer zorunludur.")]
    [Range(0, 10, ErrorMessage = "Değer 0 ile 10 arasında olmalıdır.")]
    [Display(Name = "Değer")]
    public int Value { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}

public class AppSettingListItemViewModel
{
    public int RecId { get; set; }
    public string SettingGroup { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public int Value { get; set; }
    public bool IsActive { get; set; }
}

public class AppSettingListViewModel
{
    public bool ShowPassive { get; set; }
    public IReadOnlyList<AppSettingListItemViewModel> Items { get; set; } = Array.Empty<AppSettingListItemViewModel>();
}
