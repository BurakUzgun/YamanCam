using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YamanCam.Web.Models;

public class AppProductTransferEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Evrak no zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Evrak No")]
    public string VoucherNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tarih zorunludur.")]
    [Display(Name = "Tarih")]
    [DataType(DataType.Date)]
    public DateTime VoucherDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Giren şube seçilmelidir.")]
    [Display(Name = "Giren Şube")]
    public int? InWorkPlaceId { get; set; }

    [Required(ErrorMessage = "Çıkan şube seçilmelidir.")]
    [Display(Name = "Çıkan Şube")]
    public int? OutWorkPlaceId { get; set; }

    [Display(Name = "İşlem Tür")]
    public string TransactionType { get; set; } = AppProductTransferTransactionTypes.DifferentProduct;

    [StringLength(50)]
    [Display(Name = "Özel Kod")]
    public string? SpecialCode { get; set; }

    [Display(Name = "Toplam Tutar")]
    public decimal TotalAmount { get; set; }

    public List<AppProductTransferLineEditViewModel> Lines { get; set; } = new()
    {
        new AppProductTransferLineEditViewModel(),
        new AppProductTransferLineEditViewModel()
    };

    public IReadOnlyList<SelectListItem> WorkPlaceOptions { get; set; } = Array.Empty<SelectListItem>();

    /// <summary>Çıkan Stok Adı listesi (tüm aktif stoklar).</summary>
    public IReadOnlyList<SelectListItem> StockOptions { get; set; } = Array.Empty<SelectListItem>();

    /// <summary>Giren / Aktarılan Stok Adı listesi (yalnızca türü Mamül olan stoklar).</summary>
    public IReadOnlyList<SelectListItem> ProductStockOptions { get; set; } = Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> StockUnitOptions { get; set; } = Array.Empty<SelectListItem>();
}

public class AppProductTransferLineEditViewModel
{
    public int RecId { get; set; }

    public int LineNo { get; set; }

    [Display(Name = "Çıkan Stok Adı")]
    public int OutStockId { get; set; }

    [Display(Name = "Giren Stok Adı")]
    public int InStockId { get; set; }

    [Display(Name = "Stok Birim")]
    public int? StockUnitId { get; set; }

    [Display(Name = "Adet")]
    public decimal Quantity { get; set; }

    [Display(Name = "Birim Fiyat")]
    public decimal UnitPrice { get; set; }

    [Display(Name = "Toplam Fiyat")]
    public decimal TotalPrice { get; set; }
}
