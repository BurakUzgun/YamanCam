namespace YamanCam.Web.Models;

public class AppStockIssueListItemViewModel
{
    public int RecId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string? WorkPlaceName { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? SpecialCode { get; set; }
    public decimal TotalAmount { get; set; }
}

public class AppStockIssueListViewModel
{
    public bool ShowDeleted { get; set; }
    public IReadOnlyList<AppStockIssueListItemViewModel> Items { get; set; } = Array.Empty<AppStockIssueListItemViewModel>();
}
