using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_ProductionDefinition")]
public class AppProductionDefinition
{
    [Key]
    public int RecId { get; set; }

    public int ProductStockId { get; set; }

    public int RawMaterialStockId { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal ProductionQuantity { get; set; }

    public int WorkPlaceId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(ProductStockId))]
    public AppStock? ProductStock { get; set; }

    [ForeignKey(nameof(RawMaterialStockId))]
    public AppStock? RawMaterialStock { get; set; }

    [ForeignKey(nameof(WorkPlaceId))]
    public AppWorkPlace? WorkPlace { get; set; }
}
