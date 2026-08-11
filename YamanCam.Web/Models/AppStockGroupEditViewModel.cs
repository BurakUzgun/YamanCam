using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YamanCam.Web.Models;

public class AppStockGroupEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Stok grup kodu zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Stok Grup Kodu")]
    public string GroupCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Stok grup adı zorunludur.")]
    [StringLength(200)]
    [Display(Name = "Stok Grup Adı")]
    public string GroupName { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Stok Alış Muhasebe Kodu")]
    public string? PurchaseAccountCode { get; set; }

    [StringLength(50)]
    [Display(Name = "Stok Satış Muhasebe Kodu")]
    public string? SalesAccountCode { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<SelectListItem> AccountCodeOptions { get; set; } = Array.Empty<SelectListItem>();
}

public class AppStockGroupListItemViewModel
{
    public int RecId { get; set; }
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? PurchaseAccountCode { get; set; }
    public string? SalesAccountCode { get; set; }
    public bool IsActive { get; set; }
}

public class AppStockGroupListViewModel
{
    public bool ShowPassive { get; set; }
    public IReadOnlyList<AppStockGroupListItemViewModel> Items { get; set; } = Array.Empty<AppStockGroupListItemViewModel>();
}
