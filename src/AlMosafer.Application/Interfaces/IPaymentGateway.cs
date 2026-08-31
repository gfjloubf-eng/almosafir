using AlMosafer.Application.DTOs.Payments;

namespace AlMosafer.Application.Interfaces;

public interface IPaymentGateway
{
    Task<GatewayChargeResult> ChargeAsync(decimal amount, string reference);
}
