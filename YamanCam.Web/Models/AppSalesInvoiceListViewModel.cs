namespace YamanCam.Web.Models;

public class AppSalesInvoiceListItemViewModel
{
    public int RecId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? CompanyName { get; set; }
    public string? WorkPlaceName { get; set; }
    public string? AccountName { get; set; }
    public string? Description { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalAmountTRY { get; set; }
}

public class AppSalesInvoiceListViewModel
{
    public bool ShowDeleted { get; set; }
    public IReadOnlyList<AppSalesInvoiceListItemViewModel> Items { get; set; } = Array.Empty<AppSalesInvoiceListItemViewModel>();
}
