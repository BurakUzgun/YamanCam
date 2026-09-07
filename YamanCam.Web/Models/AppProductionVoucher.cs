using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

/// <summary>
/// Stok Ürün Üretim Fişi başlığı. Seçilen şubede hammaddeden mamül üretimini temsil eder.
/// Her satırda tüketilen hammadde + fire oranı ve üretilen mamül miktarı yer alır. Fiş
/// üzerinde döviz/KDV/tutar yoktur. Yapı AppStockOpening / AppStockIssue ile aynıdır.
/// </summary>
[Table("App_ProductionVoucher")]
public class AppProductionVoucher
{
    [Key]
    public int RecId { get; set; }

    [StringLength(50)]
    public string VoucherNo { get; set; } = string.Empty;

    /// <summary>Fiş tarihi.</summary>
    public DateTime VoucherDate { get; set; }

    /// <summary>Üretim tarihi (işlemin fiilen yapıldığı tarih).</summary>
    public DateTime ProductionDate { get; set; }

    public int? WorkPlaceId { get; set; }

    [StringLength(30)]
    public string TransactionType { get; set; } = AppProductionVoucherTransactionTypes.Uretim;

    [StringLength(50)]
    public string? SpecialCode { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(WorkPlaceId))]
    public AppWorkPlace? WorkPlace { get; set; }

    public List<AppProductionVoucherLine> Lines { get; set; } = new();
}

public static class AppProductionVoucherTransactionTypes
{
    public const string Uretim = "Üretim";
    public const string FasonUretim = "Fason Üretim";
    public const string Diger = "Diğer";

    public static readonly string[] All = { Uretim, FasonUretim, Diger };
}
