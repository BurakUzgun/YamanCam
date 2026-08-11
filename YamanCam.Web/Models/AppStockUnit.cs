using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_StockUnit")]
public class AppStockUnit
{
    [Key]
    public int RecId { get; set; }

    [StringLength(20)]
    public string UnitCode { get; set; } = string.Empty;

    [StringLength(100)]
    public string UnitName { get; set; } = string.Empty;

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }
}
