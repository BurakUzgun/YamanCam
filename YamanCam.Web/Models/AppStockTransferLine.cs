using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_StockTransferLine")]
public class AppStockTransferLine
{
    [Key]
    public int RecId { get; set; }

    public int TransferId { get; set; }

    public int LineNo { get; set; }

    public int StockId { get; set; }

    public int? StockUnitId { get; set; }

    [Column(TypeName = "decimal(28,10)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(28,10)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(28,10)")]
    public decimal TotalPrice { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(TransferId))]
    public AppStockTransfer? Transfer { get; set; }

    [ForeignKey(nameof(StockId))]
    public AppStock? Stock { get; set; }

    [ForeignKey(nameof(StockUnitId))]
    public AppStockUnit? StockUnit { get; set; }
}
