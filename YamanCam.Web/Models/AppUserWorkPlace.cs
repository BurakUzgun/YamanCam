using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_UserWorkPlace")]
public class AppUserWorkPlace
{
    [Key]
    public int RecId { get; set; }

    public int UserId { get; set; }

    public int WorkPlaceId { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }

    [ForeignKey(nameof(WorkPlaceId))]
    public AppWorkPlace? WorkPlace { get; set; }
}
