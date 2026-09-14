using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_ProductTransferLine")]
public class AppProductTransferLine
{
    [Key]
    public int RecId { get; set; }

    public int TransferId { get; set; }

    public int LineNo { get; set; }

    /// <summary>Çıkan Stok (çıkan şubeden çıkan malzeme).</summary>
    public int OutStockId { get; set; }

    /// <summary>Giren / Aktarılan Stok (giren şubeye giren mamül).</summary>
    public int InStockId { get; set; }

    public int? StockUnitId { get; set; }

    [Column(TypeName = "decimal(28,10)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(28,10)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(28,10)")]
    public decimal TotalPrice { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(TransferId))]
    public AppProductTransfer? Transfer { get; set; }

    [ForeignKey(nameof(OutStockId))]
    public AppStock? OutStock { get; set; }

    [ForeignKey(nameof(InStockId))]
    public AppStock? InStock { get; set; }

    [ForeignKey(nameof(StockUnitId))]
    public AppStockUnit? StockUnit { get; set; }
}
