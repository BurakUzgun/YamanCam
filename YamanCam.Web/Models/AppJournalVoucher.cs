using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_JournalVoucher")]
public class AppJournalVoucher
{
    [Key]
    public int RecId { get; set; }

    [StringLength(20)]
    public string VoucherType { get; set; } = AppJournalVoucherTypes.Mahsup;

    [StringLength(50)]
    public string VoucherNo { get; set; } = string.Empty;

    public DateTime VoucherDate { get; set; }

    public int? CompanyId { get; set; }

    public int? WorkPlaceId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDebit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCredit { get; set; }

    [StringLength(3)]
    public string CurrencyCode { get; set; } = "TRY";

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public AppCompany? Company { get; set; }

    [ForeignKey(nameof(WorkPlaceId))]
    public AppWorkPlace? WorkPlace { get; set; }

    public List<AppJournalVoucherLine> Lines { get; set; } = new();
}

public static class AppJournalVoucherTypes
{
    public const string Mahsup = "Mahsup";
    public const string Tediye = "Tediye";
    public const string Tahsilat = "Tahsilat";
}
