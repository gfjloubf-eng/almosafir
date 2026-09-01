using AlMosafer.Infrastructure.Services;
using Xunit;

namespace AlMosafer.Tests;

/// <summary>اختبارات قلب توقيع التذاكر (P43) — نقيّة بلا قاعدة: HMAC ثابت + مقارنة زمنية.</summary>
public class TicketSignatureTest
{
    private const string Secret = "AB12CD34EF56"; // رمز تجريبي لا علاقة له بأي قاعدة

    [Fact]
    public void Roundtrip_Valid_Signature_Passes()
    {
        var sig = TicketSignatureService.ComputeSignature(Secret, 42);
        Assert.StartsWith("v1-", sig);
        Assert.True(TicketSignatureService.VerifySignature(Secret, 42, sig));
    }

    [Fact]
    public void Signature_Is_Url_Safe_Base64Url()
    {
        var sig = TicketSignatureService.ComputeSignature(Secret, 7)[3..];
        Assert.DoesNotContain('+', sig);
        Assert.DoesNotContain('/', sig);
        Assert.DoesNotContain('=', sig); // الحشو يُحذف — آمن داخل رابط QR
    }

    [Fact]
    public void Wrong_Secret_Fails()
    {
        var sig = TicketSignatureService.ComputeSignature(Secret, 42);
        Assert.False(TicketSignatureService.VerifySignature("سر-آخر-تماماً", 42, sig));
    }

    [Fact]
    public void Wrong_BookingId_Fails()
    {
        var sig = TicketSignatureService.ComputeSignature(Secret, 42);
        Assert.False(TicketSignatureService.VerifySignature(Secret, 43, sig)); // تبديل الرقم لا يمرّر
    }

    [Fact]
    public void Tampered_Character_Fails()
    {
        var sig = TicketSignatureService.ComputeSignature(Secret, 42);
        var idx = sig.Length - 2;
        var flipped = sig[..idx] + (sig[idx] == 'A' ? 'B' : 'A') + sig[(idx + 1)..];
        Assert.False(TicketSignatureService.VerifySignature(Secret, 42, flipped)); // حرف واحد يكفي للكشف
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("v0-AAAA")]        // إصدار غير معروف
    [InlineData("v1-!!!###")]      // base64url فاسد
    [InlineData("v1-QQ")]          // طول خاطئ
    public void Malformed_Or_Missing_Signature_Fails_Safely(string? sig)
    {
        Assert.False(TicketSignatureService.VerifySignature(Secret, 42, sig));
    }

    [Fact]
    public void Same_Input_Deterministic_Different_Bookings_Diverge()
    {
        var a = TicketSignatureService.ComputeSignature(Secret, 1);
        var b = TicketSignatureService.ComputeSignature(Secret, 1);
        var c = TicketSignatureService.ComputeSignature(Secret, 2);
        Assert.Equal(a, b);      // ثبات — نفس التذكرة نفس الرمز دائماً
        Assert.NotEqual(a, c);   // تباين — لا توقيعان متشابهان بين حجزين
    }
}
