using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_VatWithholdingDefinition")]
public class AppVatWithholdingDefinition
{
    [Key]
    public int RecId { get; set; }

    [StringLength(20)]
    public string WithholdingCode { get; set; } = string.Empty;

    [StringLength(100)]
    public string WithholdingName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5,2)")]
    public decimal VatRate { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal WithholdingRate { get; set; }

    [StringLength(50)]
    public string? AccountCode { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }
}
