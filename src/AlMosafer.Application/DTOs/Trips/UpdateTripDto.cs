using System.ComponentModel.DataAnnotations;
using AlMosafer.Domain.Enums;

namespace AlMosafer.Application.DTOs.Trips;

public class UpdateTripDto
{
    public int TripId { get; set; }

    [Required(ErrorMessage = "مدينة الانطلاق مطلوبة")]
    public string FromCity { get; set; } = string.Empty;

    public string FromLocation { get; set; } = string.Empty;

    [Required(ErrorMessage = "مدينة الوصول مطلوبة")]
    public string ToCity { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاريخ ووقت الرحلة مطلوب")]
    public DateTime TripTime { get; set; }

    [Range(1, 10, ErrorMessage = "عدد المقاعد يجب أن يكون بين 1 و 10")]
    public int Seats { get; set; }

    [Range(100, 1000000, ErrorMessage = "سعر المقعد غير صحيح")]
    public decimal PricePerSeat { get; set; }

    public string? Description { get; set; }
    public string? VehicleInfo { get; set; }
    public TripStatus Status { get; set; }
}
