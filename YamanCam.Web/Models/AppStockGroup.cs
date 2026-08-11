using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_StockGroup")]
public class AppStockGroup
{
    [Key]
    public int RecId { get; set; }

    [StringLength(50)]
    public string GroupCode { get; set; } = string.Empty;

    [StringLength(200)]
    public string GroupName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? PurchaseAccountCode { get; set; }

    [StringLength(50)]
    public string? SalesAccountCode { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }
}
