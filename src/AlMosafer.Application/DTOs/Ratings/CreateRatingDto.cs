using System.ComponentModel.DataAnnotations;

namespace AlMosafer.Application.DTOs.Ratings;

public class CreateRatingDto
{
    [Required(ErrorMessage = "معرف الرحلة مطلوب.")]
    public int TripId { get; set; }

    [Required(ErrorMessage = "قيمة التقييم مطلوبة.")]
    [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5 نجوم.")]
    public int Value { get; set; }

    [StringLength(500, ErrorMessage = "المراجعة النصية لا تتجاوز 500 حرف.")]
    public string? Comment { get; set; }
}
