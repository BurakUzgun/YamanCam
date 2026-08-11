using System.ComponentModel.DataAnnotations;

namespace YamanCam.Web.Models;

public class LicenseRequestViewModel
{
    [Required(ErrorMessage = "Isyeri bilgisi zorunludur.")]
    [Display(Name = "Isyeri Bilgisi")]
    public string HotelName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Firma bilgisi zorunludur.")]
    [Display(Name = "Firma Bilgisi")]
    public string CompanyName { get; set; } = string.Empty;

    public int RoomCount { get; set; } = 1;

    [Required(ErrorMessage = "Lisans kullanicisi zorunludur.")]
    [Display(Name = "Lisans Kullanicisi")]
    public string LicenceUser { get; set; } = string.Empty;
}
