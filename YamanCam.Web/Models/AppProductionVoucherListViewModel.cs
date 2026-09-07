namespace YamanCam.Web.Models;

public class AppProductionVoucherListItemViewModel
{
    public int RecId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public DateTime ProductionDate { get; set; }
    public string? WorkPlaceName { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? SpecialCode { get; set; }
    public int LineCount { get; set; }
}

public class AppProductionVoucherListViewModel
{
    public bool ShowDeleted { get; set; }
    public IReadOnlyList<AppProductionVoucherListItemViewModel> Items { get; set; } = Array.Empty<AppProductionVoucherListItemViewModel>();
}
