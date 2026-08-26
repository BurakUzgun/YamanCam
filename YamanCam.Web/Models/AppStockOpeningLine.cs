using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_StockOpeningLine")]
public class AppStockOpeningLine
{
    [Key]
    public int RecId { get; set; }

    public int OpeningId { get; set; }

    public int LineNo { get; set; }

    public int StockId { get; set; }

    public int? StockUnitId { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(OpeningId))]
    public AppStockOpening? Opening { get; set; }

    [ForeignKey(nameof(StockId))]
    public AppStock? Stock { get; set; }

    [ForeignKey(nameof(StockUnitId))]
    public AppStockUnit? StockUnit { get; set; }
}
