using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_StockCountLine")]
public class AppStockCountLine
{
    [Key]
    public int RecId { get; set; }

    public int CountId { get; set; }

    public int LineNo { get; set; }

    public int StockId { get; set; }

    /// <summary>Sayım miktarı.</summary>
    [Column(TypeName = "decimal(28,10)")]
    public decimal CountQuantity { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(CountId))]
    public AppStockCount? Count { get; set; }

    [ForeignKey(nameof(StockId))]
    public AppStock? Stock { get; set; }
}
