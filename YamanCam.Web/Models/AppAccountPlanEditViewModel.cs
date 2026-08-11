using System.ComponentModel.DataAnnotations;

namespace YamanCam.Web.Models;

public class AppAccountPlanEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Hesap kodu zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Hesap Kodu")]
    public string AccountCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hesap adı zorunludur.")]
    [StringLength(200)]
    [Display(Name = "Hesap Adı")]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Döviz seçimi zorunludur.")]
    [StringLength(3)]
    [Display(Name = "Döviz")]
    public string CurrencyCode { get; set; } = "TRY";

    [StringLength(30)]
    [Display(Name = "Hesap Türü")]
    public string? AccountType { get; set; }

    [StringLength(1)]
    [Display(Name = "Bakiye Türü")]
    public string? BalanceType { get; set; }

    [StringLength(50)]
    [Display(Name = "Özel Kod")]
    public string? SpecialCode { get; set; }

    [StringLength(100)]
    [Display(Name = "Vergi Dairesi")]
    public string? Tax { get; set; }

    [StringLength(20)]
    [Display(Name = "Vergi No")]
    public string? TaxNo { get; set; }

    [StringLength(500)]
    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [StringLength(100)]
    [Display(Name = "Şehir")]
    public string? City { get; set; }

    [StringLength(100)]
    [Display(Name = "Ülke")]
    public string? Country { get; set; }

    [StringLength(150)]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string? EMail { get; set; }

    [StringLength(100)]
    [Display(Name = "Yetkili")]
    public string? Person { get; set; }

    [StringLength(30)]
    [Display(Name = "Telefon")]
    public string? Tel { get; set; }

    [StringLength(30)]
    [Display(Name = "Faks")]
    public string? Fax { get; set; }

    [StringLength(30)]
    [Display(Name = "GSM")]
    public string? Gsm { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}

public class AppAccountPlanListItemViewModel
{
    public int RecId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public string? AccountType { get; set; }
    public string? BalanceType { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
}

public class AppAccountPlanListViewModel
{
    public bool ShowPassive { get; set; }
    public IReadOnlyList<AppAccountPlanListItemViewModel> Items { get; set; } = Array.Empty<AppAccountPlanListItemViewModel>();
}
