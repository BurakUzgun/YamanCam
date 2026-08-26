namespace YamanCam.Web.Models;

public class AppStockOpeningListItemViewModel
{
    public int RecId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string? WorkPlaceName { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? SpecialCode { get; set; }
    public decimal TotalAmount { get; set; }
}

public class AppStockOpeningListViewModel
{
    public bool ShowDeleted { get; set; }
    public IReadOnlyList<AppStockOpeningListItemViewModel> Items { get; set; } = Array.Empty<AppStockOpeningListItemViewModel>();
}
