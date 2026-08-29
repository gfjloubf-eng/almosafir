namespace AlMosafer.Application.DTOs.Reports;

public class TimeSeriesPointDto
{
    public string PeriodLabel { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public decimal Amount { get; set; }
}
