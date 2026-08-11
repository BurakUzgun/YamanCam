namespace YamanCam.Web.Models;

public class AppCustomsFreightInvoiceListItemViewModel
{
    public int RecId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string? WorkPlaceName { get; set; }
    public string? AccountName { get; set; }
    public string? LinkedPurchaseInvoiceNo { get; set; }
    public decimal NetAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal WithholdingAmount { get; set; }
    public decimal NetVatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; }
    public decimal CurrencyAmount { get; set; }
}

public class AppCustomsFreightInvoiceListViewModel
{
    public bool ShowDeleted { get; set; }
    public IReadOnlyList<AppCustomsFreightInvoiceListItemViewModel> Items { get; set; } = Array.Empty<AppCustomsFreightInvoiceListItemViewModel>();
}
