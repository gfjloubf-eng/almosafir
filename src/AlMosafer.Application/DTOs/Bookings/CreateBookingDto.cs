using System.ComponentModel.DataAnnotations;
using AlMosafer.Domain.Enums;

namespace AlMosafer.Application.DTOs.Bookings;

public class CreateBookingDto
{
    [Required]
    public int TripId { get; set; }

    [Range(1, 10, ErrorMessage = "عدد المقاعد المحجوزة يجب أن يكون 1 على الأقل")]
    public int SeatsBooked { get; set; } = 1;

    /// <summary>طريقة الدفع: تجريبي فوري أو نقداً عند الركوب</summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.MockOnline;
}
