namespace YamanCam.Web.Models;

public class AppJournalVoucherListItemViewModel
{
    public int RecId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string? CompanyName { get; set; }
    public string? WorkPlaceName { get; set; }
    public string? Description { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
}

public class AppJournalVoucherListViewModel
{
    public bool ShowDeleted { get; set; }
    public IReadOnlyList<AppJournalVoucherListItemViewModel> Items { get; set; } = Array.Empty<AppJournalVoucherListItemViewModel>();
}
