namespace AlMosafer.Application.DTOs.Reports;

/// <summary>
/// إحصاءات عامة خفيفة للواجهة الرئيسية — أرقام مصداقية تُعرض للزائر الأول
/// (استعلامات COUNT بسيطة بلا تحميل كيانات كاملة، على عكس ملخص تقارير الإدارة الثقيل).
/// </summary>
public class PublicStatsDto
{
    /// <summary>رحلات متاحة الآن (حالتها Open).</summary>
    public int ActiveTrips { get; set; }

    /// <summary>عدد السائقين المسجلين/المعتمدين.</summary>
    public int DriversCount { get; set; }

    /// <summary>حجوزات مؤكدة أو صعد أصحابها للمركبة.</summary>
    public int CompletedBookings { get; set; }
}
