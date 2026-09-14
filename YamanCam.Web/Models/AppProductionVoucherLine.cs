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
    [Column(TypeName = "decimal(28,10)")]
    public decimal RawMaterialQuantity { get; set; }

    /// <summary>
    /// Hammaddenin birim maliyeti. Alış Faturaları ve Satış İade Faturaları'ndaki (aynı
    /// hammadde, son üretim tarihinden sonraki) net tutar/miktar toplamlarının ağırlıklı
    /// ortalamasından hesaplanır.
    /// </summary>
    [Column(TypeName = "decimal(28,10)")]
    public decimal RawMaterialUnitPrice { get; set; }

    /// <summary>Hammadde toplam tutarı (RawMaterialQuantity x RawMaterialUnitPrice).</summary>
    [Column(TypeName = "decimal(28,10)")]
    public decimal RawMaterialNetAmount { get; set; }

    /// <summary>Fire oranı (%).</summary>
    [Column(TypeName = "decimal(13,10)")]
    public decimal WasteRate { get; set; }

    /// <summary>Üretilen mamül miktarı.</summary>
    [Column(TypeName = "decimal(28,10)")]
    public decimal ProductQuantity { get; set; }

    /// <summary>Mamülün birim maliyeti; hammaddenin birim maliyetiyle aynı değeri taşır.</summary>
    [Column(TypeName = "decimal(28,10)")]
    public decimal ProductUnitPrice { get; set; }

    /// <summary>Mamül toplam tutarı (ProductQuantity x ProductUnitPrice).</summary>
    [Column(TypeName = "decimal(28,10)")]
    public decimal ProductNetAmount { get; set; }

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
