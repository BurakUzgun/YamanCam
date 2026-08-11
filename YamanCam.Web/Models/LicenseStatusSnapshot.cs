namespace YamanCam.Web.Models;

public class LicenseStatusSnapshot
{
    public bool HasRequest { get; set; }
    public int? RequestId { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public DateTime? EndOfDate { get; set; }
    public DateTime? LastCheckedUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}
