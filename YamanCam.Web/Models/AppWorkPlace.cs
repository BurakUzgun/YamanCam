using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_WorkPlace")]
public class AppWorkPlace
{
    [Key]
    public int RecId { get; set; }

    [StringLength(50)]
    public string? WorkPlaceCode { get; set; }

    [StringLength(200)]
    public string? WorkPlaceName { get; set; }

    public int CompanyId { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public AppCompany? Company { get; set; }
}
