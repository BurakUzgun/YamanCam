using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

/// <summary>
/// Şubeler Arası Transfer Fişi başlığı. Çıkan şubeden giren şubeye stok aktarımını temsil eder.
/// Fiş üzerinde döviz/KDV yoktur; yalnızca miktar x birim fiyat = toplam fiyat hesaplanır.
/// </summary>
[Table("App_StockTransfer")]
public class AppStockTransfer
{
    [Key]
    public int RecId { get; set; }

    [StringLength(50)]
    public string VoucherNo { get; set; } = string.Empty;

    public DateTime VoucherDate { get; set; }

    /// <summary>Giren Şube (stokun aktarıldığı şube).</summary>
    public int? InWorkPlaceId { get; set; }

    /// <summary>Çıkan Şube (stokun çıktığı şube).</summary>
    public int? OutWorkPlaceId { get; set; }

    [StringLength(30)]
    public string TransactionType { get; set; } = AppStockTransferTransactionTypes.Transfer;

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

    public List<AppStockTransferLine> Lines { get; set; } = new();
}

public static class AppStockTransferTransactionTypes
{
    public const string Transfer = "Transfer";

    public static readonly string[] All = { Transfer };
}
