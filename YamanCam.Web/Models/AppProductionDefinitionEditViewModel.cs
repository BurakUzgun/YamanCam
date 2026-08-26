using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YamanCam.Web.Models;

public class AppProductionDefinitionEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Mamul seçilmelidir.")]
    [Display(Name = "Mamul Adı")]
    public int ProductStockId { get; set; }

    [Required(ErrorMessage = "Hammadde seçilmelidir.")]
    [Display(Name = "Hammadde Adı")]
    public int RawMaterialStockId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Üretim miktarı sıfırdan büyük olmalıdır.")]
    [Display(Name = "Üretim Miktar")]
    public decimal ProductionQuantity { get; set; } = 1;

    [Required(ErrorMessage = "Şube seçilmelidir.")]
    [Display(Name = "Şube Seçimi")]
    public int WorkPlaceId { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public List<SelectListItem> StockOptions { get; set; } = new();
    public List<SelectListItem> WorkPlaceOptions { get; set; } = new();
}

public class AppProductionDefinitionListItemViewModel
{
    public int RecId { get; set; }
    public string ProductStockName { get; set; } = string.Empty;
    public string RawMaterialStockName { get; set; } = string.Empty;
    public decimal ProductionQuantity { get; set; }
    public string WorkPlaceName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class AppProductionDefinitionListViewModel
{
    public bool ShowPassive { get; set; }
    public IReadOnlyList<AppProductionDefinitionListItemViewModel> Items { get; set; } = Array.Empty<AppProductionDefinitionListItemViewModel>();
}
