using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

/// <summary>
/// Sayım Kaydı başlığı. Bir şubede belirli bir dönem (başlangıç - bitiş tarihi) için
/// yapılan stok sayımını temsil eder. Satırlarda her stok için sayım miktarı tutulur.
/// Fiş üzerinde döviz/KDV/tutar yoktur. Yapı AppStockOpening / AppStockIssue ile aynıdır.
/// </summary>
[Table("App_StockCount")]
public class AppStockCount
{
    [Key]
    public int RecId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int? WorkPlaceId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(WorkPlaceId))]
    public AppWorkPlace? WorkPlace { get; set; }

    public List<AppStockCountLine> Lines { get; set; } = new();
}
