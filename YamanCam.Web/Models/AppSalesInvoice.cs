using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_SalesInvoice")]
public class AppSalesInvoice
{
    [Key]
    public int RecId { get; set; }

    [StringLength(50)]
    public string InvoiceNo { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; }

    public int? CompanyId { get; set; }

    public int? WorkPlaceId { get; set; }

    public int AccountId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(3)]
    public string CurrencyCode { get; set; } = "TRY";

    [Column(TypeName = "decimal(18,6)")]
    public decimal ExchangeRate { get; set; } = 1m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal VatAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetAmountTRY { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal VatAmountTRY { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmountTRY { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public AppCompany? Company { get; set; }

    [ForeignKey(nameof(WorkPlaceId))]
    public AppWorkPlace? WorkPlace { get; set; }

    [ForeignKey(nameof(AccountId))]
    public AppAccountPlan? Account { get; set; }

    public List<AppSalesInvoiceLine> Lines { get; set; } = new();
}
