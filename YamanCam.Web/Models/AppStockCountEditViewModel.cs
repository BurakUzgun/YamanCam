using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YamanCam.Web.Models;

public class AppStockCountEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
    [Display(Name = "Başlangıç Tarih")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
    [Display(Name = "Bitiş Tarihi")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Şube seçilmelidir.")]
    [Display(Name = "Şube")]
    public int? WorkPlaceId { get; set; }

    public List<AppStockCountLineEditViewModel> Lines { get; set; } = new()
    {
        new AppStockCountLineEditViewModel()
    };

    public IReadOnlyList<SelectListItem> WorkPlaceOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> StockOptions { get; set; } = Array.Empty<SelectListItem>();
}

public class AppStockCountLineEditViewModel
{
    public int RecId { get; set; }

    public int LineNo { get; set; }

    [Display(Name = "Stok Adı")]
    public int StockId { get; set; }

    [Display(Name = "Sayım Miktar")]
    public decimal CountQuantity { get; set; }
}
