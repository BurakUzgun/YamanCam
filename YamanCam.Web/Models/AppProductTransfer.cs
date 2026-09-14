using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

/// <summary>
/// Şubeler Arası Transfer (Farklı Ürün) fişi başlığı. Çıkan şubeden çıkan bir stok,
/// giren şubeye farklı bir ürün (aktarılan ürün) olarak girer. Her satırda çıkan stok
/// ile aktarılan (giren) stok ayrı seçilir. Fiş üzerinde döviz/KDV yoktur; yalnızca
/// miktar x birim fiyat = toplam fiyat hesaplanır. Yapı AppStockTransfer ile aynıdır,
/// tek fark satırda ikinci (aktarılan) stok alanının bulunmasıdır.
/// </summary>
[Table("App_ProductTransfer")]
public class AppProductTransfer
{
    [Key]
    public int RecId { get; set; }

    [StringLength(50)]
    public string VoucherNo { get; set; } = string.Empty;

    public DateTime VoucherDate { get; set; }

    /// <summary>Giren Şube (aktarılan ürünün girdiği şube).</summary>
    public int? InWorkPlaceId { get; set; }

    /// <summary>Çıkan Şube (stokun çıktığı şube).</summary>
    public int? OutWorkPlaceId { get; set; }

    [StringLength(40)]
    public string TransactionType { get; set; } = AppProductTransferTransactionTypes.DifferentProduct;

    [StringLength(50)]
    public string? SpecialCode { get; set; }

    [Column(TypeName = "decimal(28,10)")]
    public decimal TotalAmount { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(InWorkPlaceId))]
    public AppWorkPlace? InWorkPlace { get; set; }

    [ForeignKey(nameof(OutWorkPlaceId))]
    public AppWorkPlace? OutWorkPlace { get; set; }

    public List<AppProductTransferLine> Lines { get; set; } = new();
}

public static class AppProductTransferTransactionTypes
{
    public const string DifferentProduct = "Farklı Ürün Transfer";

    public static readonly string[] All = { DifferentProduct };
}
