using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_StockOpening")]
public class AppStockOpening
{
    [Key]
    public int RecId { get; set; }

    [StringLength(50)]
    public string VoucherNo { get; set; } = string.Empty;

    public DateTime VoucherDate { get; set; }

    public int? WorkPlaceId { get; set; }

    [StringLength(30)]
    public string TransactionType { get; set; } = AppStockOpeningTransactionTypes.IlkGiris;

    [StringLength(50)]
    public string? SpecialCode { get; set; }

    [Column(TypeName = "decimal(28,10)")]
    public decimal TotalAmount { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(WorkPlaceId))]
    public AppWorkPlace? WorkPlace { get; set; }

    public List<AppStockOpeningLine> Lines { get; set; } = new();
}

public static class AppStockOpeningTransactionTypes
{
    public const string IlkGiris = "İlk Giriş";
    public const string DevirGirisi = "Devir Girişi";
    public const string SayimFazlasi = "Sayım Fazlası";
    public const string Diger = "Diğer";

    public static readonly string[] All = { IlkGiris, DevirGirisi, SayimFazlasi, Diger };
}
