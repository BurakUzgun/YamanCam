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
    [StringLength(200)]
    [Display(Name = "Değer")]
    public string Value { get; set; } = string.Empty;

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}

public class AppSettingListItemViewModel
{
    public int RecId { get; set; }
    public string SettingGroup { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class AppSettingListViewModel
{
    public bool ShowPassive { get; set; }
    public IReadOnlyList<AppSettingListItemViewModel> Items { get; set; } = Array.Empty<AppSettingListItemViewModel>();
}
