using AlMosafer.Application.DTOs.Reports;

namespace AlMosafer.Application.Interfaces;

/// <summary>
/// قراءة ذيل سجلات Serilog المهيكلة لعرضها للأدمن — عرض آمن بلا وصول مباشر لنظام الملفات من الطبقات العليا.
/// </summary>
public interface ILogTailService
{
    /// <summary>آخر الأحداث من أحدث ملف يومي؛ الأحدث أولاً. ترشيح اختياري بالمستوى (Error/Warning/Information…).</summary>
    Task<IReadOnlyList<LogEntryDto>> GetLatestAsync(int max = 200, string? level = null);
}
