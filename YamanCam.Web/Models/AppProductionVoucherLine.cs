using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_ProductionVoucherLine")]
public class AppProductionVoucherLine
{
    [Key]
    public int RecId { get; set; }

    public int VoucherId { get; set; }

    public int LineNo { get; set; }

    /// <summary>Hammadde stok kartı.</summary>
    public int RawMaterialStockId { get; set; }

    /// <summary>Tüketilen hammadde miktarı.</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal RawMaterialQuantity { get; set; }

    /// <summary>Fire oranı (%).</summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal WasteRate { get; set; }

    /// <summary>Üretilen mamül miktarı.</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal ProductQuantity { get; set; }

    /// <summary>Mamül stok kartı.</summary>
    public int ProductStockId { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(VoucherId))]
    public AppProductionVoucher? Voucher { get; set; }

    [ForeignKey(nameof(RawMaterialStockId))]
    public AppStock? RawMaterialStock { get; set; }

    [ForeignKey(nameof(ProductStockId))]
    public AppStock? ProductStock { get; set; }
}
