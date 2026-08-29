namespace AlMosafer.Application.DTOs.Reports;

public class PaymentStatisticsDto
{
    public int TotalTransactionsCount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal AverageTransactionAmount => PaidTransactionsCount > 0 ? Math.Round(TotalPaidAmount / PaidTransactionsCount, 2) : 0;
    public int PaidTransactionsCount { get; set; }
    public int PendingTransactionsCount { get; set; }
    public int FailedTransactionsCount { get; set; }
}
