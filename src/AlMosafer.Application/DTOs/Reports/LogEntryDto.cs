namespace AlMosafer.Application.DTOs.Reports;

/// <summary>سطر سجل مهيكل معروض في صفحة «الرادار» للأدمن (P46 الفرعي).</summary>
public sealed record LogEntryDto(DateTimeOffset? Timestamp, string Level, string Message, string? Exception);
