using AlMosafer.Application.DTOs.Payments;
using AlMosafer.Application.Interfaces;

namespace AlMosafer.Infrastructure.Services;

/// <summary>
/// بوابة دفع محاكية — يعود دائماً بالنجاح.
/// تنويه صادق: لا يوجد أي تكامل دفع حقيقي حالياً؛ هذه البوابة جسر للمستقبل.
/// </summary>
public class MockPaymentGateway : IPaymentGateway
{
    public Task<GatewayChargeResult> ChargeAsync(decimal amount, string reference)
    {
        return Task.FromResult(new GatewayChargeResult
        {
            Success = true,
            TransactionId = $"TXN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            Message = "دفع تجريبي ناجح (محاكاة — لا يوجد خصم حقيقي)"
        });
    }
}
