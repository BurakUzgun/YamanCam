namespace YamanCam.Web.Models;

public class AppLogListViewModel
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public string? Level { get; set; }
    public string? LogType { get; set; }
    public string? UserName { get; set; }
    public string? SearchText { get; set; }
    public IReadOnlyList<AppLog> Logs { get; set; } = Array.Empty<AppLog>();
}
