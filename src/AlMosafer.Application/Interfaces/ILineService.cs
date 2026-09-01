using AlMosafer.Application.DTOs.Lines;

namespace AlMosafer.Application.Interfaces;

public interface ILineService
{
    Task<IEnumerable<LineSummaryDto>> GetActiveLinesAsync(string? city = null);
    Task<IReadOnlyList<string>> GetActiveCitiesAsync();
    Task<LineDetailsDto?> GetLineDetailsAsync(int lineId);

    // إدارة الشبكة (لوحة الإدارة)
    Task<IEnumerable<LineSummaryDto>> GetAllLinesAsync();
    Task<(bool Success, string Message)> CreateLineAsync(string name, string city, string? description);
    Task<(bool Success, string Message)> AddStopAsync(int lineId, string name, int orderIndex);
    Task<(bool Success, string Message)> AddScheduleAsync(int lineId, int dayOfWeek, string timeText);
    Task<(bool Success, string Message)> SetLineActiveAsync(int lineId, bool isActive);
    Task<(bool Success, string Message)> DeleteLineAsync(int lineId);
}
