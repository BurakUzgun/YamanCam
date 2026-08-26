using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YamanCam.Web.Models;

public class AppVatWithholdingDefinitionEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Tevkifat kodu zorunludur.")]
    [StringLength(20)]
    [Display(Name = "Tevkifat Kodu")]
    public string WithholdingCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tevkifat adı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Tevkifat Adı")]
    public string WithholdingName { get; set; } = string.Empty;

    [Required(ErrorMessage = "KDV oranı zorunludur.")]
    [Range(0, 100, ErrorMessage = "KDV oranı 0 ile 100 arasında olmalıdır.")]
    [Display(Name = "KDV Oranı (%)")]
    public decimal VatRate { get; set; }

    [Required(ErrorMessage = "Tevkifat oranı zorunludur.")]
    [Range(0, 100, ErrorMessage = "Tevkifat oranı 0 ile 100 arasında olmalıdır.")]
    [Display(Name = "Tevkifat Oranı (%)")]
    public decimal WithholdingRate { get; set; }

    [StringLength(50)]
    [Display(Name = "Tevkifatın Gideceği Hesap Kodu")]
    public string? AccountCode { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<SelectListItem> AccountCodeOptions { get; set; } = Array.Empty<SelectListItem>();
}

public class AppVatWithholdingDefinitionListItemViewModel
{
    public int RecId { get; set; }
    public string WithholdingCode { get; set; } = string.Empty;
    public string WithholdingName { get; set; } = string.Empty;
    public decimal VatRate { get; set; }
    public decimal WithholdingRate { get; set; }
    public string? AccountCode { get; set; }
    public bool IsActive { get; set; }
}

public class AppVatWithholdingDefinitionListViewModel
{
    public bool ShowPassive { get; set; }
    public IReadOnlyList<AppVatWithholdingDefinitionListItemViewModel> Items { get; set; } = Array.Empty<AppVatWithholdingDefinitionListItemViewModel>();
}
