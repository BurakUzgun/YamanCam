using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_AccountPlan")]
public class AppAccountPlan
{
    [Key]
    public int RecId { get; set; }

    [StringLength(50)]
    public string AccountCode { get; set; } = string.Empty;

    [StringLength(200)]
    public string AccountName { get; set; } = string.Empty;

    public int? ParentAccountId { get; set; }

    public int? LevelNo { get; set; }

    [StringLength(30)]
    public string? AccountType { get; set; }

    [StringLength(1)]
    public string? BalanceType { get; set; }

    [StringLength(3)]
    public string CurrencyCode { get; set; } = "TRY";

    [StringLength(50)]
    public string? SpecialCode { get; set; }

    [StringLength(100)]
    public string? Tax { get; set; }

    [StringLength(20)]
    public string? TaxNo { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(150)]
    public string? EMail { get; set; }

    [StringLength(100)]
    public string? Person { get; set; }

    [StringLength(30)]
    public string? Tel { get; set; }

    [StringLength(30)]
    public string? Fax { get; set; }

    [StringLength(30)]
    public string? Gsm { get; set; }

    public bool? IsDetail { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(ParentAccountId))]
    public AppAccountPlan? ParentAccount { get; set; }
}
