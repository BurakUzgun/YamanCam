namespace YamanCam.Web.Models;

public class AppStockMergeListItemViewModel
{
    public int RecId { get; set; }
    public DateTime PeriodDate { get; set; }
    public string? WorkPlaceName { get; set; }
    public DateTime ProcessDate { get; set; }
}

public class AppStockMergeListViewModel
{
    public bool ShowDeleted { get; set; }
    public IReadOnlyList<AppStockMergeListItemViewModel> Items { get; set; } = Array.Empty<AppStockMergeListItemViewModel>();
}
