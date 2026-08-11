using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_User")]
public class AppUser
{
    [Key]
    public int RecId { get; set; }

    [StringLength(50)]
    public string? Code { get; set; }

    [StringLength(100)]
    public string? NameSurname { get; set; }

    [StringLength(100)]
    public string? Password { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    public bool? IsRight { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    [StringLength(1000)]
    public string? HotelsId { get; set; }
}
