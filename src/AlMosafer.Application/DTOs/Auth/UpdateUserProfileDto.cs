using System.ComponentModel.DataAnnotations;

namespace AlMosafer.Application.DTOs.Auth;

public class UpdateUserProfileDto
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب.")]
    [StringLength(100, ErrorMessage = "الاسم لا يتجاوز 100 حرف.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "رقم الهاتف لا يتجاوز 20 رقم.")]
    public string? Phone { get; set; }

    [StringLength(100, ErrorMessage = "اسم المدينة لا يتجاوز 100 حرف.")]
    public string? City { get; set; }

    [StringLength(50, ErrorMessage = "موديل السيارة لا يتجاوز 50 حرف.")]
    public string? VehicleModel { get; set; }

    [StringLength(20, ErrorMessage = "رقم اللوحة لا يتجاوز 20 حرف.")]
    public string? PlateNumber { get; set; }

    public int? VehicleYear { get; set; }
}
