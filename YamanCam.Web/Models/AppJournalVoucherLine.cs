using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_JournalVoucherLine")]
public class AppJournalVoucherLine
{
    [Key]
    public int RecId { get; set; }

    public int VoucherId { get; set; }

    public int LineNo { get; set; }

    public int AccountId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(28,10)")]
    public decimal DebitAmount { get; set; }

    [Column(TypeName = "decimal(28,10)")]
    public decimal CreditAmount { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(VoucherId))]
    public AppJournalVoucher? Voucher { get; set; }

    [ForeignKey(nameof(AccountId))]
    public AppAccountPlan? Account { get; set; }
}
