using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_CustomsFreightInvoice")]
public class AppCustomsFreightInvoice
{
    [Key]
    public int RecId { get; set; }

    public DateTime InvoiceDate { get; set; }

    public int? WorkPlaceId { get; set; }

    [StringLength(30)]
    public string? InvoiceKind { get; set; }

    public int AccountId { get; set; }

    [StringLength(50)]
    public string InvoiceNo { get; set; } = string.Empty;

    [StringLength(50)]
    public string? SpecialCode { get; set; }

    public int? LinkedPurchaseInvoiceId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal VatAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal WithholdingAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetVatAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrencyAmount { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal ExchangeRate { get; set; } = 1m;

    [StringLength(3)]
    public string CurrencyCode { get; set; } = "TRY";

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(WorkPlaceId))]
    public AppWorkPlace? WorkPlace { get; set; }

    [ForeignKey(nameof(AccountId))]
    public AppAccountPlan? Account { get; set; }

    [ForeignKey(nameof(LinkedPurchaseInvoiceId))]
    public AppPurchaseInvoice? LinkedPurchaseInvoice { get; set; }

    public List<AppCustomsFreightInvoiceLine> Lines { get; set; } = new();
}
