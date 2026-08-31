namespace YamanCam.Web.Models;

public class AppStockTransferListItemViewModel
{
    public int RecId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string? InWorkPlaceName { get; set; }
    public string? OutWorkPlaceName { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? SpecialCode { get; set; }
    public decimal TotalAmount { get; set; }
}

public class AppStockTransferListViewModel
{
    public bool ShowDeleted { get; set; }
    public IReadOnlyList<AppStockTransferListItemViewModel> Items { get; set; } = Array.Empty<AppStockTransferListItemViewModel>();
}
