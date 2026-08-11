namespace YamanCam.Web.Models;

public class AppAccountStatementItemViewModel
{
    public DateTime VoucherDate { get; set; }
    public string VoucherType { get; set; } = string.Empty;
    public string VoucherTypeLabel { get; set; } = string.Empty;
    public string VoucherControllerName { get; set; } = string.Empty;
    public int VoucherRecId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? WorkPlaceName { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal RunningBalance { get; set; }
}

public class AppAccountStatementViewModel
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public bool IsGroupAccount { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance { get; set; }
    public IReadOnlyList<AppAccountStatementItemViewModel> Items { get; set; } = Array.Empty<AppAccountStatementItemViewModel>();
}
