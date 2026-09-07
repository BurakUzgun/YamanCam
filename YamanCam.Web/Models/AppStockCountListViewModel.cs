namespace YamanCam.Web.Models;

public class AppStockCountListItemViewModel
{
    public int RecId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? WorkPlaceName { get; set; }
    public int LineCount { get; set; }
}

public class AppStockCountListViewModel
{
    public bool ShowDeleted { get; set; }
    public IReadOnlyList<AppStockCountListItemViewModel> Items { get; set; } = Array.Empty<AppStockCountListItemViewModel>();
}
