using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_VatDefinition")]
public class AppVatDefinition
{
    [Key]
    public int RecId { get; set; }

    [StringLength(20)]
    public string VatCode { get; set; } = string.Empty;

    [StringLength(100)]
    public string VatName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5,2)")]
    public decimal VatRate { get; set; }

    [StringLength(50)]
    public string? PurchaseAccountCode { get; set; }

    [StringLength(50)]
    public string? SalesAccountCode { get; set; }

    [StringLength(50)]
    public string? PurchaseReturnAccountCode { get; set; }

    [StringLength(50)]
    public string? SalesReturnAccountCode { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }
}
