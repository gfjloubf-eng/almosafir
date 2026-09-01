using AlMosafer.Application.DTOs.Bookings;

namespace AlMosafer.Application.Interfaces;

/// <summary>
/// توقيع/تحقق تذاكر الحجز ضد التزوير (P43 التذكرة):
/// رمز سري لكل حجز في القاعدة فقط، والـQR يحمل HMAC-SHA256 مشتقاً — بلا الرمز لا يمكن تزوير توقيع.
/// </summary>
public interface ITicketSignatureService
{
    /// <summary>توقيع تذكرة لعرضها في إيصال المسافر. null إن كان الحجز أقدم من الميزة (غير موقّع).</summary>
    Task<string?> CreateSignatureAsync(int bookingId);

    /// <summary>تحقق عند المسح من جهة مخوّلة (سائق الرحلة/أدمن). لا يكشف عن السر أبداً.</summary>
    Task<TicketVerificationDto> VerifyAsync(int bookingId, string? signature);
}
