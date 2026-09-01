using AlMosafer.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace AlMosafer.Infrastructure.Services;

/// <summary>
/// بريد خام (SMTP عام عبر MailKit). فاشل-آمن: إن غابت الإعدادات يسكت بنجاح-كاذب
/// ولا يرمي استثناء — خدمة بريد اختيارية لا توقف النظام.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_configuration["Smtp:Host"]) &&
        !string.IsNullOrWhiteSpace(_configuration["Smtp:User"]);

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string toEmail, string subject, string bodyHtml)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("SMTP غير مهيأ — تخطّي إرسال البريد إلى {Email}", MaskEmail(toEmail));
            return false;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration["Smtp:FromName"] ?? "منصة المسافر", _configuration["Smtp:User"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = bodyHtml }.ToMessageBody();

            using var client = new SmtpClient();
            var port = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
            await client.ConnectAsync(_configuration["Smtp:Host"], port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_configuration["Smtp:User"], _configuration["Smtp:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "فشل إرسال بريد إلى {Email}", MaskEmail(toEmail));
            return false;
        }
    }

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        return at > 2 ? email[..2] + "***" + email[at..] : "***";
    }
}
