using System.ComponentModel.DataAnnotations;

namespace AlMosafer.Application.DTOs.Auth;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور الجديدة يجب أن لا تقل عن 6 أحرف")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد كلمة المرور الجديدة مطلوب")]
    [Compare("NewPassword", ErrorMessage = "كلمتا المرور غير متطابقتين")]
    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
