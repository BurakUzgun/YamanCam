using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_Setting")]
public class AppSetting
{
    [Key]
    public int RecId { get; set; }

    [StringLength(100)]
    public string SettingGroup { get; set; } = string.Empty;

    [StringLength(200)]
    public string Explanation { get; set; } = string.Empty;

    public int Value { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }
}
