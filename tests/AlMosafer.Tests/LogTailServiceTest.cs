using AlMosafer.Infrastructure.Services;
using Xunit;

namespace AlMosafer.Tests;

/// <summary>اختبارات «عين الرادار»: قراءة ذيل سجلات Serilog بأمان وتسامح.</summary>
public class LogTailServiceTest
{
    private static LogTailService At(string contentRoot) => new(contentRoot);

    [Fact]
    public async Task Missing_Logs_Directory_Returns_Empty_Without_Throwing()
    {
        var root = Path.Combine(Path.GetTempPath(), "alm-logtail-" + Guid.NewGuid().ToString("N"));
        var entries = await At(root).GetLatestAsync();
        Assert.Empty(entries);
    }

    [Fact]
    public async Task Parses_Serilog_Json_And_Newest_File_First_Ordered_Newest_First()
    {
        var root = Path.Combine(Path.GetTempPath(), "alm-logtail-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "logs"));
        await File.WriteAllTextAsync(Path.Combine(root, "logs", "almosafer-20260901.json"),
            "{\"Timestamp\":\"2026-09-01T10:00:00+03:00\",\"Level\":\"Information\",\"RenderedMessage\":\"قديم\"}\n");
        await File.WriteAllTextAsync(Path.Combine(root, "logs", "almosafer-20260902.json"),
            "{\"Timestamp\":\"2026-09-02T10:00:00+03:00\",\"Level\":\"Warning\",\"RenderedMessage\":\"أولاً\"}\n" +
            "{\"Timestamp\":\"2026-09-02T11:00:00+03:00\",\"Level\":\"Error\",\"RenderedMessage\":\"ثانياً\"}\n");

        var entries = await At(root).GetLatestAsync();

        Assert.Equal(2, entries.Count); // من أحدث ملف فقط
        Assert.Equal("ثانياً", entries[0].Message); // الأحدث أولاً
        Assert.Equal("Error", entries[0].Level);
        Assert.NotNull(entries[0].Timestamp);
    }

    [Fact]
    public async Task Level_Filter_Works_Case_Insensitively()
    {
        var root = Path.Combine(Path.GetTempPath(), "alm-logtail-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "logs"));
        await File.WriteAllLinesAsync(Path.Combine(root, "logs", "almosafer-20260901.json"), new[]
        {
            "{\"Level\":\"Information\",\"RenderedMessage\":\"معلومة\"}",
            "{\"Level\":\"Error\",\"RenderedMessage\":\"خطأ فادح\"}"
        });

        var onlyErrors = await At(root).GetLatestAsync(200, "error");

        Assert.Single(onlyErrors);
        Assert.Equal("خطأ فادح", onlyErrors[0].Message);
    }

    [Fact]
    public async Task Tolerates_Half_Written_Non_Json_Line_As_Text()
    {
        var root = Path.Combine(Path.GetTempPath(), "alm-logtail-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "logs"));
        await File.WriteAllTextAsync(Path.Combine(root, "logs", "almosafer-20260901.json"),
            "{\"Level\":\"Information\",\"RenderedMessage\":\"سليم\"}\n{\"Timestamp\":\"2026-09-01T10:0"); // سطر مقطوع لحظة الدوران

        var entries = await At(root).GetLatestAsync();

        Assert.Equal(2, entries.Count);
        Assert.Equal("Text", entries[0].Level); // المقطوع أولاً لأنه الأحدث
        Assert.Equal("سليم", entries[1].Message);
    }

    [Fact]
    public async Task Tail_Cap_Respected_And_Long_Exception_Truncated()
    {
        var root = Path.Combine(Path.GetTempPath(), "alm-logtail-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "logs"));
        var lines = Enumerable.Range(1, 10)
            .Select(i => $"{{\"Level\":\"Information\",\"RenderedMessage\":\"حدث {i}\"}}")
            .Append($"{{\"Level\":\"Error\",\"RenderedMessage\":\"مع استثناء\",\"Exception\":\"{new string('x', 900)}\"}}");
        await File.WriteAllLinesAsync(Path.Combine(root, "logs", "almosafer-20260901.json"), lines);

        var entries = await At(root).GetLatestAsync(max: 5);

        Assert.Equal(5, entries.Count);
        Assert.Equal("مع استثناء", entries[0].Message);
        Assert.True(entries[0].Exception!.Length <= 401); // مقتطع بأمان
        Assert.EndsWith("…", entries[0].Exception);
    }
}
