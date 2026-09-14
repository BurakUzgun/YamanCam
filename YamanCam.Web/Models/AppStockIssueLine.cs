using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_StockIssueLine")]
public class AppStockIssueLine
{
    [Key]
    public int RecId { get; set; }

    public int IssueId { get; set; }

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

    [ForeignKey(nameof(IssueId))]
    public AppStockIssue? Issue { get; set; }

    [ForeignKey(nameof(StockId))]
    public AppStock? Stock { get; set; }

    [ForeignKey(nameof(StockUnitId))]
    public AppStockUnit? StockUnit { get; set; }
}
