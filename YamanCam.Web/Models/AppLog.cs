using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YamanCam.Web.Models;

[Table("App_Log")]
public class AppLog
{
    [Key]
    public int RecId { get; set; }

    public DateTime LogDateUtc { get; set; }

    [StringLength(20)]
    public string Level { get; set; } = "Info";

    [StringLength(100)]
    public string? Category { get; set; }

    [StringLength(150)]
    public string? EventName { get; set; }

    [StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? OldValue { get; set; }

    [StringLength(4000)]
    public string? NewValue { get; set; }

    [StringLength(100)]
    public string? UserName { get; set; }

    public int? UserId { get; set; }

    [StringLength(50)]
    public string? IpAddress { get; set; }

    [StringLength(10)]
    public string? Method { get; set; }

    [StringLength(300)]
    public string? Path { get; set; }

    [StringLength(1000)]
    public string? QueryString { get; set; }

    public int? StatusCode { get; set; }

    public long? DurationMs { get; set; }
}
