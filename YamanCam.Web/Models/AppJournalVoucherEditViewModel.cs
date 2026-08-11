using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YamanCam.Web.Models;

public class AppJournalVoucherEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Fiş no zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Fiş No")]
    public string VoucherNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fiş tarihi zorunludur.")]
    [Display(Name = "Fiş Tarihi")]
    [DataType(DataType.Date)]
    public DateTime VoucherDate { get; set; } = DateTime.Today;

    [Display(Name = "Şirket")]
    public int? CompanyId { get; set; }

    [Display(Name = "Şube")]
    public int? WorkPlaceId { get; set; }

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Döviz")]
    public string CurrencyCode { get; set; } = "TRY";

    [Display(Name = "Kasa/Banka Hesabı")]
    public int? CashBankAccountId { get; set; }

    public List<AppJournalVoucherLineEditViewModel> Lines { get; set; } = new()
    {
        new AppJournalVoucherLineEditViewModel(),
        new AppJournalVoucherLineEditViewModel()
    };

    public IReadOnlyList<SelectListItem> CompanyOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> WorkPlaceOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> AccountOptions { get; set; } = Array.Empty<SelectListItem>();
}

public class AppJournalVoucherLineEditViewModel
{
    public int RecId { get; set; }

    public int LineNo { get; set; }

    [Display(Name = "Hesap")]
    public int AccountId { get; set; }

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Borç")]
    public decimal DebitAmount { get; set; }

    [Display(Name = "Alacak")]
    public decimal CreditAmount { get; set; }
}
