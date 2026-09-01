namespace AlMosafer.Application.DTOs.Bookings;

/// <summary>
/// نتيجة مسح تذكرة (P43 التذكرة الموقّعة) كما تظهر للسائق/الأدمن عند التحقق:
/// Found=الحجز موجود · Signed=له رمز توقيع · Valid=التوقيع سليم. الحجوزات الأقدم من الميزة Signed=false بصدق.
/// </summary>
public sealed record TicketVerificationDto(
    bool Found,
    bool Signed,
    bool Valid,
    string? TravelerName,
    string? Route,
    int Seats,
    string? Status,
    DateTime? TripTime);
