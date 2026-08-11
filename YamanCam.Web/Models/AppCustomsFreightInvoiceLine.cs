using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_CustomsFreightInvoiceLine")]
public class AppCustomsFreightInvoiceLine
{
    [Key]
    public int RecId { get; set; }

    public int InvoiceId { get; set; }

    public int LineNo { get; set; }

    public int AccountId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal VatRate { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal WithholdingRate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal VatAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal WithholdingAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetVatAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(InvoiceId))]
    public AppCustomsFreightInvoice? Invoice { get; set; }

    [ForeignKey(nameof(AccountId))]
    public AppAccountPlan? Account { get; set; }
}
