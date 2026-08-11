using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YamanCam.Web.Models;

public class AppVatDefinitionEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "KDV kodu zorunludur.")]
    [StringLength(20)]
    [Display(Name = "KDV Kodu")]
    public string VatCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "KDV adı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "KDV Adı")]
    public string VatName { get; set; } = string.Empty;

    [Required(ErrorMessage = "KDV oranı zorunludur.")]
    [Range(0, 100, ErrorMessage = "KDV oranı 0 ile 100 arasında olmalıdır.")]
    [Display(Name = "KDV Oranı (%)")]
    public decimal VatRate { get; set; }

    [StringLength(50)]
    [Display(Name = "Alış Muhasebe Kodu")]
    public string? PurchaseAccountCode { get; set; }

    [StringLength(50)]
    [Display(Name = "Satış Muhasebe Kodu")]
    public string? SalesAccountCode { get; set; }

    [StringLength(50)]
    [Display(Name = "Alış İade Muhasebe Kodu")]
    public string? PurchaseReturnAccountCode { get; set; }

    [StringLength(50)]
    [Display(Name = "Satış İade Muhasebe Kodu")]
    public string? SalesReturnAccountCode { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<SelectListItem> AccountCodeOptions { get; set; } = Array.Empty<SelectListItem>();
}

public class AppVatDefinitionListItemViewModel
{
    public int RecId { get; set; }
    public string VatCode { get; set; } = string.Empty;
    public string VatName { get; set; } = string.Empty;
    public decimal VatRate { get; set; }
    public string? PurchaseAccountCode { get; set; }
    public string? SalesAccountCode { get; set; }
    public string? PurchaseReturnAccountCode { get; set; }
    public string? SalesReturnAccountCode { get; set; }
    public bool IsActive { get; set; }
}

public class AppVatDefinitionListViewModel
{
    public bool ShowPassive { get; set; }
    public IReadOnlyList<AppVatDefinitionListItemViewModel> Items { get; set; } = Array.Empty<AppVatDefinitionListItemViewModel>();
}
