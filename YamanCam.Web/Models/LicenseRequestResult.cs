namespace YamanCam.Web.Models;

public class LicenseRequestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? RequestId { get; set; }
}
