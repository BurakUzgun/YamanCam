using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_SchemaVersion")]
public class AppSchemaVersion
{
    [Key]
    public int RecId { get; set; }

    public int VersionNo { get; set; }

    [StringLength(100)]
    public string ScriptName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime AppliedUtc { get; set; }
}
