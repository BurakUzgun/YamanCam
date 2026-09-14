using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

/// <summary>
/// Stok Çıkış Fişi başlığı. Seçilen şubeden stok çıkışını temsil eder. Satırlar
/// App_Stock (malzeme kartları) referans alır, her satırda seçilen malzemenin birimi
/// otomatik önerilir. Fiş üzerinde döviz/KDV yoktur; yalnızca miktar x birim fiyat =
/// toplam fiyat hesaplanır. Yapı AppStockOpening ile aynıdır.
/// </summary>
[Table("App_StockIssue")]
public class AppStockIssue
{
    [Key]
    public int RecId { get; set; }

    [StringLength(50)]
    public string VoucherNo { get; set; } = string.Empty;

    public DateTime VoucherDate { get; set; }

    public int? WorkPlaceId { get; set; }

    [StringLength(30)]
    public string TransactionType { get; set; } = AppStockIssueTransactionTypes.SarfCikisi;

    [StringLength(50)]
    public string? SpecialCode { get; set; }

    [Column(TypeName = "decimal(28,10)")]
    public decimal TotalAmount { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(WorkPlaceId))]
    public AppWorkPlace? WorkPlace { get; set; }

    public List<AppStockIssueLine> Lines { get; set; } = new();
}

public static class AppStockIssueTransactionTypes
{
    public const string SarfCikisi = "Sarf Çıkışı";
    public const string Fire = "Fire";
    public const string SayimEksigi = "Sayım Eksiği";
    public const string Diger = "Diğer";

    public static readonly string[] All = { SarfCikisi, Fire, SayimEksigi, Diger };
}
