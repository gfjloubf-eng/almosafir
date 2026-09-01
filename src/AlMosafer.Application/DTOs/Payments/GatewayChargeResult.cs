namespace AlMosafer.Application.DTOs.Payments;

/// <summary>
/// نتيجة عملية دفع عبر بوابة — تجريد عام تستطيع لاحقاً أي بوابة دفع محلية
/// (محافظ إلكترونية يمنية وغيرها) تطبيقه دون مساس بخدمات النظام الداخلية.
/// </summary>
public class GatewayChargeResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string Message { get; set; } = string.Empty;
}
