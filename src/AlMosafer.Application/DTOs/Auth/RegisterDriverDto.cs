using System.ComponentModel.DataAnnotations;

namespace AlMosafer.Application.DTOs.Auth;

public class RegisterDriverDto
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "الاسم يجب أن يكون بين 3 و 100 حرف")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن لا تقل عن 6 أحرف")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
    [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم الهاتف مطلوب للسائق")]
    [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم اللوحة مطلوب")]
    public string PlateNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "موديل السيارة مطلوب")]
    public string VehicleModel { get; set; } = string.Empty;

    [Range(1990, 2026, ErrorMessage = "سنة الصنع غير صحيحة")]
    public int? VehicleYear { get; set; }

    public string? City { get; set; }
}
