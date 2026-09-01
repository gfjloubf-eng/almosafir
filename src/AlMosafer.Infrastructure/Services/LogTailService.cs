using System.Text.Json;
using AlMosafer.Application.DTOs.Reports;
using AlMosafer.Application.Interfaces;
using Microsoft.Extensions.Hosting;

namespace AlMosafer.Infrastructure.Services;

/// <summary>
/// P46 الفرعي «عين الرادار»: ذيل آمن لملفات Serilog اليومية (logs/almosafer-*.json).
/// القواعد: أحدث ملف فقط، طابور ذيل بحد أقصى (لا تحميل للملف كله)،
/// تسامح كامل مع سطر نصف مكتوب أو غير JSON، واقتطاع الاستثناءات الطويلة.
/// </summary>
public class LogTailService : ILogTailService
{
    private const int ExceptionPreviewLimit = 400;
    private readonly string _logsDir;

    public LogTailService(IHostEnvironment env)
    {
        _logsDir = Path.Combine(env.ContentRootPath, "logs");
    }

    public Task<IReadOnlyList<LogEntryDto>> GetLatestAsync(int max = 200, string? level = null)
    {
        var result = new List<LogEntryDto>();
        try
        {
            if (!Directory.Exists(_logsDir)) return Task.FromResult<IReadOnlyList<LogEntryDto>>(result);

            var newest = Directory.EnumerateFiles(_logsDir, "almosafer-*.json")
                .OrderByDescending(f => f, StringComparer.Ordinal)
                .FirstOrDefault();
            if (newest is null) return Task.FromResult<IReadOnlyList<LogEntryDto>>(result);

            // ذيل فعال: طابور آخر N سطراً (الملف قد يكتمل يومياً لآلاف الأسطر)
            var tail = new Queue<string>(max);
            foreach (var line in File.ReadLines(newest))
            {
                if (tail.Count == max) tail.Dequeue();
                tail.Enqueue(line);
            }

            foreach (var line in tail.Reverse()) // الأحدث أولاً أمام عين الأدمن
            {
                var entry = Parse(line);
                if (level is not null &&
                    !string.Equals(entry.Level, level, StringComparison.OrdinalIgnoreCase)) continue;
                result.Add(entry);
            }
        }
        catch (IOException)
        {
            // الملف يُخَلَّق أو يُدوَّر لحظتها — نُعيد ما جُمع وتُعاد المحاولة بتحديث الصفحة
        }
        return Task.FromResult<IReadOnlyList<LogEntryDto>>(result);
    }

    internal static LogEntryDto Parse(string line)
    {
        try
        {
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;
            string? Get(params string[] names)
            {
                foreach (var n in names)
                    if (root.TryGetProperty(n, out var v) && v.ValueKind == JsonValueKind.String)
                        return v.GetString();
                return null;
            }

            DateTimeOffset? ts = DateTimeOffset.TryParse(Get("Timestamp", "@t"), out var p) ? p : null;
            var level = Get("Level", "@l") ?? "Information";
            var message = Get("RenderedMessage", "@mt", "MessageTemplate") ?? line;
            var ex = Get("Exception", "@x");
            if (ex is not null && ex.Length > ExceptionPreviewLimit) ex = ex[..ExceptionPreviewLimit] + "…";
            return new LogEntryDto(ts, level, message, ex);
        }
        catch (JsonException)
        {
            // سطر نصف مكتوب أو نص حر — يُعرض كنص خام بمسؤولية
            return new LogEntryDto(null, "Text", line, null);
        }
    }
}
