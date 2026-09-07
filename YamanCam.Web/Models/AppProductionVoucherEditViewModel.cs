using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YamanCam.Web.Models;

public class AppProductionVoucherEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Evrak no zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Evrak No")]
    public string VoucherNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tarih zorunludur.")]
    [Display(Name = "Tarih")]
    [DataType(DataType.Date)]
    public DateTime VoucherDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Üretim tarihi zorunludur.")]
    [Display(Name = "Üretim Tarihi")]
    [DataType(DataType.Date)]
    public DateTime ProductionDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Şube seçilmelidir.")]
    [Display(Name = "Şube")]
    public int? WorkPlaceId { get; set; }

    [Required(ErrorMessage = "İşlem türü seçilmelidir.")]
    [Display(Name = "İşlem Tür")]
    public string TransactionType { get; set; } = AppProductionVoucherTransactionTypes.Uretim;

    [StringLength(50)]
    [Display(Name = "Özel Kod")]
    public string? SpecialCode { get; set; }

    public List<AppProductionVoucherLineEditViewModel> Lines { get; set; } = new()
    {
        new AppProductionVoucherLineEditViewModel(),
        new AppProductionVoucherLineEditViewModel()
    };

    public IReadOnlyList<SelectListItem> WorkPlaceOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> TransactionTypeOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> StockOptions { get; set; } = Array.Empty<SelectListItem>();
}

public class AppProductionVoucherLineEditViewModel
{
    public int RecId { get; set; }

    public int LineNo { get; set; }

    [Display(Name = "Hammadde Adı")]
    public int RawMaterialStockId { get; set; }

    [Display(Name = "Miktar")]
    public decimal RawMaterialQuantity { get; set; }

    [Display(Name = "Fire Oran")]
    public decimal WasteRate { get; set; }

    [Display(Name = "Miktar")]
    public decimal ProductQuantity { get; set; }

    [Display(Name = "Mamul Adı")]
    public int ProductStockId { get; set; }
}
