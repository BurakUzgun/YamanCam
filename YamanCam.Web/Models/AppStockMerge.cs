using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

/// <summary>
/// Stokları Birleştir işlemi kaydı. Seçilen dönem ve şube için stokları birleştirme işleminin
/// izini tutar. <see cref="PeriodDate"/> kullanıcının verdiği dönem tarihi,
/// <see cref="ProcessDate"/> işlemin fiilen çalıştırıldığı tarihtir.
/// </summary>
[Table("App_StockMerge")]
public class AppStockMerge
{
    [Key]
    public int RecId { get; set; }

    /// <summary>Kullanıcının seçtiği dönem tarihi.</summary>
    public DateTime PeriodDate { get; set; }

    public int? WorkPlaceId { get; set; }

    /// <summary>Birleştirme işleminin fiilen yapıldığı tarih/saat.</summary>
    public DateTime ProcessDate { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(WorkPlaceId))]
    public AppWorkPlace? WorkPlace { get; set; }
}
