using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YamanCam.Web.Models;

public class AppStockMergeEditViewModel
{
    public int RecId { get; set; }

    [Required(ErrorMessage = "Dönem tarihi zorunludur.")]
    [Display(Name = "Dönem Seçin")]
    [DataType(DataType.Date)]
    public DateTime PeriodDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Şube seçilmelidir.")]
    [Display(Name = "Şube Seçin")]
    public int? WorkPlaceId { get; set; }

    [Display(Name = "İşlem Tarihi")]
    public DateTime? ProcessDate { get; set; }

    public IReadOnlyList<SelectListItem> WorkPlaceOptions { get; set; } = Array.Empty<SelectListItem>();
}
