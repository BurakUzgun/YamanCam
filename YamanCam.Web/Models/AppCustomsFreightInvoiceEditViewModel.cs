using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YamanCam.Web.Models;

public class AppCustomsFreightInvoiceEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Tarih zorunludur.")]
    [Display(Name = "Tarih")]
    [DataType(DataType.Date)]
    public DateTime InvoiceDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Şube seçilmelidir.")]
    [Display(Name = "Şube")]
    public int? WorkPlaceId { get; set; }

    [StringLength(30)]
    [Display(Name = "Tür")]
    public string? InvoiceKind { get; set; }

    [Required(ErrorMessage = "Cari ünvanı seçilmelidir.")]
    [Display(Name = "Cari Ünvanı")]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "Fatura no zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Fatura No")]
    public string InvoiceNo { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Özel Kod")]
    public string? SpecialCode { get; set; }

    [Display(Name = "Stok Fat No")]
    public int? LinkedPurchaseInvoiceId { get; set; }

    [Display(Name = "Matrah")]
    public decimal NetAmount { get; set; }

    [Display(Name = "KDV Tutar")]
    public decimal VatAmount { get; set; }

    [Display(Name = "Tevkifat Tutar")]
    public decimal WithholdingAmount { get; set; }

    [Display(Name = "NET KDV Tutar")]
    public decimal NetVatAmount { get; set; }

    [Display(Name = "Genel Tutar")]
    public decimal TotalAmount { get; set; }

    [Display(Name = "Döviz Tutar")]
    public decimal CurrencyAmount { get; set; }

    [Display(Name = "Döviz Kuru")]
    public decimal ExchangeRate { get; set; } = 1m;

    [Required(ErrorMessage = "Döviz kodu zorunludur.")]
    [Display(Name = "Döviz Kodu")]
    public string CurrencyCode { get; set; } = "TRY";

    public List<AppCustomsFreightInvoiceLineEditViewModel> Lines { get; set; } = new();

    public IReadOnlyList<SelectListItem> WorkPlaceOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> CariAccountOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> ExpenseAccountOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> PurchaseInvoiceOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> WithholdingDefinitionOptions { get; set; } = Array.Empty<SelectListItem>();
}

public class AppCustomsFreightInvoiceLineEditViewModel
{
    public int RecId { get; set; }

    public int LineNo { get; set; }

    [Display(Name = "Hesap")]
    public int AccountId { get; set; }

    [Display(Name = "Tutar")]
    public decimal Amount { get; set; }

    [Display(Name = "Kdv Oran")]
    public decimal VatRate { get; set; }

    [Display(Name = "Tevkifat")]
    public int? WithholdingDefinitionId { get; set; }

    [Display(Name = "Kdv İnd. Oran")]
    public decimal WithholdingRate { get; set; }

    public decimal VatAmount { get; set; }
    public decimal WithholdingAmount { get; set; }
    public decimal NetVatAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
