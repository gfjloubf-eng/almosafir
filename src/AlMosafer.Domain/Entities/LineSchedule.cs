namespace AlMosafer.Domain.Entities;

public class LineSchedule
{
    public int Id { get; set; }
    public int LineId { get; set; }
    /// <summary>يوم الدوام الأسبوعي: 0=الأحد .. 6=السبت (متوافق مع DayOfWeek)</summary>
    public int DayOfWeek { get; set; }
    public TimeSpan DepartureTime { get; set; }

    public RouteLine Line { get; set; } = null!;
}
