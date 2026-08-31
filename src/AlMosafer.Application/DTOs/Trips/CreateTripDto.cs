using System.ComponentModel.DataAnnotations;

namespace AlMosafer.Application.DTOs.Trips;

public class CreateTripDto
{
    [Required(ErrorMessage = "مدينة الانطلاق مطلوبة")]
    public string FromCity { get; set; } = string.Empty;

    public string FromLocation { get; set; } = string.Empty;

    [Required(ErrorMessage = "مدينة الوصول مطلوبة")]
    public string ToCity { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاريخ ووقت الرحلة مطلوب")]
    public DateTime TripTime { get; set; } = DateTime.Now.AddDays(1);

    [Range(1, 10, ErrorMessage = "عدد المقاعد يجب أن يكون بين 1 و 10")]
    public int Seats { get; set; } = 4;

    [Range(100, 1000000, ErrorMessage = "سعر المقعد يجب أن يكون أكبر من 100 ريال")]
    public decimal PricePerSeat { get; set; } = 10000.00m;

    public string? Description { get; set; }

    public string? VehicleInfo { get; set; }
}
