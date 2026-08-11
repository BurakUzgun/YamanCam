using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_Company")]
public class AppCompany
{
    [Key]
    public int RecId { get; set; }

    [StringLength(50)]
    public string? CompanyCode { get; set; }

    [StringLength(200)]
    public string? CompanyName { get; set; }

    [StringLength(20)]
    public string? TaxNumber { get; set; }

    [StringLength(100)]
    public string? TaxOffice { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }
}
