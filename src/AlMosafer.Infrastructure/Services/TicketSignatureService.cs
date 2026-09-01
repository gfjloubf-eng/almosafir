using System.Security.Cryptography;
using System.Text;
using AlMosafer.Application.DTOs.Bookings;
using AlMosafer.Application.Interfaces;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMosafer.Infrastructure.Services;

/// <summary>
/// تنفيذ توقيع التذاكر (P43): HMAC-SHA256 بمفتاح = الرمز السري الخاص بالحجز (لا يحفظ في الـQR).
/// الصيغة: v1-&lt;base64url(hmac(key=secret, msg="ticket:v1:{id}"))&gt; — مقارنة زمنية ثابتة ضد هجمات التوقيت.
/// كل مسح ناجح/فاشل يُدوَّن في سجل الرادار لمتابعة أنماط التزوير.
/// </summary>
public class TicketSignatureService : ITicketSignatureService
{
    private readonly AlMosaferDbContext _db;
    private readonly ILogger<TicketSignatureService> _logger;

    public TicketSignatureService(AlMosaferDbContext db, ILogger<TicketSignatureService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<string?> CreateSignatureAsync(int bookingId)
    {
        var secret = await _db.Bookings
            .Where(b => b.Id == bookingId)
            .Select(b => b.TicketSecret)
            .FirstOrDefaultAsync();
        return secret is null ? null : ComputeSignature(secret, bookingId);
    }

    public async Task<TicketVerificationDto> VerifyAsync(int bookingId, string? signature)
    {
        var row = await _db.Bookings.AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => new
            {
                b.TicketSecret,
                b.SeatsBooked,
                b.Status,
                TravelerName = b.Traveler.Name,
                Route = b.Trip.FromCity + " ← " + b.Trip.ToCity,
                TripTime = (DateTime?)b.Trip.TripTime
            })
            .FirstOrDefaultAsync();

        if (row is null)
        {
            _logger.LogWarning("مسح تذكرة لحجز غير موجود: {BookingId}", bookingId);
            return new TicketVerificationDto(false, false, false, null, null, 0, null, null);
        }

        var signed = row.TicketSecret is not null;
        var valid = signed && VerifySignature(row.TicketSecret!, bookingId, signature);

        if (!signed)
            _logger.LogInformation("سُحبت تذكرة قديمة غير موقعة للحجز {BookingId} — تُعتمد عبر كشف الصعود", bookingId);
        else if (!valid)
            _logger.LogWarning("⚠ تذكرة مزوّرة/تالفة للحجز {BookingId} — توقيع غير مطابق", bookingId);

        return new TicketVerificationDto(
            true, signed, valid, row.TravelerName, row.Route, row.SeatsBooked, row.Status.ToString(), row.TripTime);
    }

    // ─── قلب التوقيع (نقي ثابت — قابل للاختبار دون قاعدة) ───

    public static string ComputeSignature(string secret, int bookingId)
    {
        var mac = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes($"ticket:v1:{bookingId}"));
        return "v1-" + Convert.ToBase64String(mac).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static bool VerifySignature(string secret, int bookingId, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature) || !signature.StartsWith("v1-", StringComparison.Ordinal))
            return false;
        var expectedMac = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes($"ticket:v1:{bookingId}"));

        byte[] providedMac;
        try
        {
            providedMac = Base64UrlDecode(signature[3..]);
        }
        catch (FormatException)
        {
            return false;
        }
        return providedMac.Length == expectedMac.Length &&
               CryptographicOperations.FixedTimeEquals(providedMac, expectedMac);
    }

    private static byte[] Base64UrlDecode(string s)
    {
        var b64 = s.Replace('-', '+').Replace('_', '/');
        b64 += new string('=', (4 - b64.Length % 4) % 4);
        return Convert.FromBase64String(b64);
    }
}
