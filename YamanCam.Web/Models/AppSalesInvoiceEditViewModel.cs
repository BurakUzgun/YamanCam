using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YamanCam.Web.Models;

public class AppSalesInvoiceEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Fatura no zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Fatura No")]
    public string InvoiceNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fatura tarihi zorunludur.")]
    [Display(Name = "Fatura Tarihi")]
    [DataType(DataType.Date)]
    public DateTime InvoiceDate { get; set; } = DateTime.Today;

    [Display(Name = "Şirket")]
    public int? CompanyId { get; set; }

    [Required(ErrorMessage = "Şube seçilmelidir.")]
    [Display(Name = "Şube")]
    public int? WorkPlaceId { get; set; }

    [Required(ErrorMessage = "Fatura carisi seçilmelidir.")]
    [Display(Name = "Fatura Carisi")]
    public int AccountId { get; set; }

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Döviz seçimi zorunludur.")]
    [Display(Name = "Döviz")]
    public string CurrencyCode { get; set; } = "TRY";

    [Display(Name = "Döviz Kuru")]
    public decimal ExchangeRate { get; set; } = 1m;

    [Display(Name = "Mal Bedeli (Döviz)")]
    public decimal NetAmount { get; set; }

    [Display(Name = "KDV (Döviz)")]
    public decimal VatAmount { get; set; }

    [Display(Name = "Tevkifat (Döviz)")]
    public decimal WithholdingAmount { get; set; }

    [Display(Name = "NET KDV (Döviz)")]
    public decimal NetVatAmount { get; set; }

    [Display(Name = "Genel Toplam (Döviz)")]
    public decimal TotalAmount { get; set; }

    [Display(Name = "Mal Bedeli (TL)")]
    public decimal NetAmountTRY { get; set; }

    [Display(Name = "KDV (TL)")]
    public decimal VatAmountTRY { get; set; }

    [Display(Name = "Tevkifat (TL)")]
    public decimal WithholdingAmountTRY { get; set; }

    [Display(Name = "NET KDV (TL)")]
    public decimal NetVatAmountTRY { get; set; }

    [Display(Name = "Genel Toplam (TL)")]
    public decimal TotalAmountTRY { get; set; }

    public List<AppSalesInvoiceLineEditViewModel> Lines { get; set; } = new()
    {
        new AppSalesInvoiceLineEditViewModel(),
        new AppSalesInvoiceLineEditViewModel()
    };

    public IReadOnlyList<SelectListItem> CompanyOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> WorkPlaceOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> AccountOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> StockOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> WithholdingDefinitionOptions { get; set; } = Array.Empty<SelectListItem>();
}

public class AppSalesInvoiceLineEditViewModel
{
    public int RecId { get; set; }

    public int LineNo { get; set; }

    [Display(Name = "Malzeme")]
    public int StockId { get; set; }

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Miktar")]
    public decimal Quantity { get; set; }

    [Display(Name = "Birim Fiyat")]
    public decimal UnitPrice { get; set; }

    [Display(Name = "KDV %")]
    public decimal VatRate { get; set; }

    [Display(Name = "Tevkifat")]
    public int? WithholdingDefinitionId { get; set; }

    [Display(Name = "Tevkifat %")]
    public decimal WithholdingRate { get; set; }

    public decimal NetAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal WithholdingAmount { get; set; }
    public decimal NetVatAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
