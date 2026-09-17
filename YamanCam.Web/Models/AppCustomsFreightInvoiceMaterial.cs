using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_CustomsFreightInvoiceMaterial")]
public class AppCustomsFreightInvoiceMaterial
{
    [Key]
    public int RecId { get; set; }

    public int InvoiceId { get; set; }

    public int LineNo { get; set; }

    public int StockId { get; set; }

    [Column(TypeName = "decimal(28,10)")]
    public decimal Quantity { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(InvoiceId))]
    public AppCustomsFreightInvoice? Invoice { get; set; }

    [ForeignKey(nameof(StockId))]
    public AppStock? Stock { get; set; }
}
