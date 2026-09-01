namespace AlMosafer.Application.Interfaces;

public interface IEmailService
{
    /// <summary>يرسل بريداً HTML. يعيد false بأمان إن لم تُهيَّأ إعدادات SMTP (وضع فاشل-آمن).</summary>
    Task<bool> SendAsync(string toEmail, string subject, string bodyHtml);
    bool IsConfigured { get; }
}
