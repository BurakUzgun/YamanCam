using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YamanCam.Web.Models;

public class AppStockEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Stok kodu zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Stok Kodu")]
    public string StockCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Stok adı zorunludur.")]
    [StringLength(200)]
    [Display(Name = "Stok Adı")]
    public string StockName { get; set; } = string.Empty;

    [Display(Name = "Stok Grubu")]
    public int? StockGroupId { get; set; }

    [Display(Name = "Stok Birimi")]
    public int? StockUnitId { get; set; }

    [Display(Name = "Alış KDV")]
    public int? PurchaseVatId { get; set; }

    [Display(Name = "Satış KDV")]
    public int? SalesVatId { get; set; }

    [StringLength(50)]
    [Display(Name = "Barkod")]
    public string? Barcode { get; set; }

    [StringLength(30)]
    [Display(Name = "Stok Türü")]
    public string? StockType { get; set; }

    [Display(Name = "Üretim Ağırlığı")]
    public decimal? ProductionWeight { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<SelectListItem> StockGroupOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> StockUnitOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> VatOptions { get; set; } = Array.Empty<SelectListItem>();
}

public class AppStockListItemViewModel
{
    public int RecId { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public string StockName { get; set; } = string.Empty;
    public string? StockGroupName { get; set; }
    public string? StockUnitCode { get; set; }
    public string? PurchaseVatLabel { get; set; }
    public string? SalesVatLabel { get; set; }
    public string? Barcode { get; set; }
    public string? StockType { get; set; }
    public decimal? ProductionWeight { get; set; }
    public bool IsActive { get; set; }
}

public class AppStockListViewModel
{
    public bool ShowPassive { get; set; }
    public IReadOnlyList<AppStockListItemViewModel> Items { get; set; } = Array.Empty<AppStockListItemViewModel>();
}
