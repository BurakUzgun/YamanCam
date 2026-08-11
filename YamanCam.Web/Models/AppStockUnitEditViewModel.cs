using System.ComponentModel.DataAnnotations;

namespace YamanCam.Web.Models;

public class AppStockUnitEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Birim kodu zorunludur.")]
    [StringLength(20)]
    [Display(Name = "Birim Kodu")]
    public string UnitCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Birim adı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Birim Adı")]
    public string UnitName { get; set; } = string.Empty;

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}

public class AppStockUnitListItemViewModel
{
    public int RecId { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class AppStockUnitListViewModel
{
    public bool ShowPassive { get; set; }
    public IReadOnlyList<AppStockUnitListItemViewModel> Items { get; set; } = Array.Empty<AppStockUnitListItemViewModel>();
}
