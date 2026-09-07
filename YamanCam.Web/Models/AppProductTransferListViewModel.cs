namespace YamanCam.Web.Models;

public class AppProductTransferListItemViewModel
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

public class AppProductTransferListViewModel
{
    public bool ShowDeleted { get; set; }
    public IReadOnlyList<AppProductTransferListItemViewModel> Items { get; set; } = Array.Empty<AppProductTransferListItemViewModel>();
}
