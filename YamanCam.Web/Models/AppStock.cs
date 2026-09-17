using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_Stock")]
public class AppStock
{
    [Key]
    public int RecId { get; set; }

    [StringLength(50)]
    public string StockCode { get; set; } = string.Empty;

    [StringLength(200)]
    public string StockName { get; set; } = string.Empty;

    public int? StockGroupId { get; set; }

    public int? StockUnitId { get; set; }

    public int? PurchaseVatId { get; set; }

    public int? SalesVatId { get; set; }

    [StringLength(50)]
    public string? Barcode { get; set; }

    [StringLength(30)]
    public string? StockType { get; set; }

    [Column(TypeName = "decimal(28,10)")]
    public decimal? ProductionWeight { get; set; }

    public int? MergeStockId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(StockGroupId))]
    public AppStockGroup? StockGroup { get; set; }

    [ForeignKey(nameof(StockUnitId))]
    public AppStockUnit? StockUnit { get; set; }

    [ForeignKey(nameof(PurchaseVatId))]
    public AppVatDefinition? PurchaseVat { get; set; }

    [ForeignKey(nameof(SalesVatId))]
    public AppVatDefinition? SalesVat { get; set; }

    [ForeignKey(nameof(MergeStockId))]
    public AppStock? MergeStock { get; set; }
}
