namespace YamanCam.Web.Models;

public class AppUserListItemViewModel
{
    public int RecId { get; set; }
    public string? Code { get; set; }
    public string? NameSurname { get; set; }
    public bool? IsRight { get; set; }
    public bool? IsActive { get; set; }
    public string WorkPlaceSummary { get; set; } = string.Empty;
}
