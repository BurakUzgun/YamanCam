using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_SalesInvoiceLine")]
public class AppSalesInvoiceLine
{
    [Key]
    public int RecId { get; set; }

    public int InvoiceId { get; set; }

    public int LineNo { get; set; }

    public int StockId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal VatRate { get; set; }

    public int? WithholdingDefinitionId { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal WithholdingRate { get; set; }

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

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(InvoiceId))]
    public AppSalesInvoice? Invoice { get; set; }

    [ForeignKey(nameof(StockId))]
    public AppStock? Stock { get; set; }

    [ForeignKey(nameof(WithholdingDefinitionId))]
    public AppVatWithholdingDefinition? WithholdingDefinition { get; set; }
}
