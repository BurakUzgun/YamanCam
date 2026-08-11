using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_UserRight")]
public class AppUserRight
{
    [Key]
    public int RecId { get; set; }

    public int UserId { get; set; }

    public int? WorkPlaceId { get; set; }

    [StringLength(100)]
    public string RightCode { get; set; } = string.Empty;

    [StringLength(200)]
    public string? RightName { get; set; }

    public bool? IsAllowed { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }

    [ForeignKey(nameof(WorkPlaceId))]
    public AppWorkPlace? WorkPlace { get; set; }
}
