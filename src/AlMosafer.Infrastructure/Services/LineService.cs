using AlMosafer.Application.DTOs.Lines;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Entities;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class LineService : ILineService
{
    private readonly AlMosaferDbContext _dbContext;

    private static readonly string[] ArabicDays =
        { "الأحد", "الاثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت" };

    public LineService(AlMosaferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<LineSummaryDto>> GetActiveLinesAsync(string? city = null)
    {
        var query = _dbContext.RouteLines
            .AsNoTracking()
            .Where(l => l.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(city))
        {
            var c = city.Trim();
            query = query.Where(l => l.City == c);
        }

        return await query
            .OrderBy(l => l.City).ThenBy(l => l.Name)
            .Select(l => new LineSummaryDto
            {
                Id = l.Id,
                Name = l.Name,
                City = l.City,
                StopsCount = l.Stops.Count,
                SchedulesCount = l.Schedules.Count
            })
            .ToListAsync();
    }

    public Task<IReadOnlyList<string>> GetActiveCitiesAsync()
    {
        return _dbContext.RouteLines
            .AsNoTracking()
            .Where(l => l.IsActive)
            .Select(l => l.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync()
            .ContinueWith(t => (IReadOnlyList<string>)t.Result);
    }

    public async Task<LineDetailsDto?> GetLineDetailsAsync(int lineId)
    {
        var line = await _dbContext.RouteLines
            .AsNoTracking()
            .Include(l => l.Stops)
            .Include(l => l.Schedules)
            .FirstOrDefaultAsync(l => l.Id == lineId && l.IsActive);

        if (line == null)
        {
            return null;
        }

        return new LineDetailsDto
        {
            Id = line.Id,
            Name = line.Name,
            City = line.City,
            Description = line.Description,
            IsActive = line.IsActive,
            Stops = line.Stops
                .OrderBy(s => s.OrderIndex)
                .Select(s => new LineStopDto { Name = s.Name, OrderIndex = s.OrderIndex })
                .ToList(),
            Schedules = line.Schedules
                .OrderBy(s => s.DayOfWeek).ThenBy(s => s.DepartureTime)
                .Select(s => new LineScheduleDto
                {
                    DayName = s.DayOfWeek >= 0 && s.DayOfWeek <= 6 ? ArabicDays[s.DayOfWeek] : "غير محدد",
                    TimeText = s.DepartureTime.ToString(@"hh\:mm")
                })
                .ToList()
        };
    }

    public async Task<IEnumerable<LineSummaryDto>> GetAllLinesAsync()
    {
        return await _dbContext.RouteLines
            .AsNoTracking()
            .OrderBy(l => l.City).ThenBy(l => l.Name)
            .Select(l => new LineSummaryDto
            {
                Id = l.Id,
                Name = l.Name + (l.IsActive ? string.Empty : " (موقوف)"),
                City = l.City,
                StopsCount = l.Stops.Count,
                SchedulesCount = l.Schedules.Count
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> CreateLineAsync(string name, string city, string? description)
    {
        var n = (name ?? string.Empty).Trim();
        var c = (city ?? string.Empty).Trim();
        if (n.Length < 3 || c.Length < 2)
        {
            return (false, "اسم الخط والمدينة مطلوبان (اسم لا يقل عن 3 أحرف).");
        }

        _dbContext.RouteLines.Add(new RouteLine
        {
            Name = n,
            City = c,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        return (true, $"أُنشئ الخط «{n}» في {c} وأصبح ظاهراً للمسافرين.");
    }

    public async Task<(bool Success, string Message)> AddStopAsync(int lineId, string name, int orderIndex)
    {
        var exists = await _dbContext.RouteLines.AnyAsync(l => l.Id == lineId);
        if (!exists)
        {
            return (false, "الخط غير موجود.");
        }
        var n = (name ?? string.Empty).Trim();
        if (n.Length < 2)
        {
            return (false, "اسم الموقف قصير جداً.");
        }

        _dbContext.LineStops.Add(new LineStop { LineId = lineId, Name = n, OrderIndex = orderIndex });
        await _dbContext.SaveChangesAsync();
        return (true, $"أُضيف الموقف «{n}».");
    }

    public async Task<(bool Success, string Message)> AddScheduleAsync(int lineId, int dayOfWeek, string timeText)
    {
        var exists = await _dbContext.RouteLines.AnyAsync(l => l.Id == lineId);
        if (!exists)
        {
            return (false, "الخط غير موجود.");
        }
        if (dayOfWeek < 0 || dayOfWeek > 6)
        {
            return (false, "رقم اليوم يجب أن يكون بين 0 (الأحد) و6 (السبت).");
        }
        if (!TimeSpan.TryParse(timeText, out var departure))
        {
            return (false, "صيغة الوقت غير صحيحة (مثال: 16:30).");
        }

        _dbContext.LineSchedules.Add(new LineSchedule { LineId = lineId, DayOfWeek = dayOfWeek, DepartureTime = departure });
        await _dbContext.SaveChangesAsync();
        return (true, $"أُضيف موعد {ArabicDays[dayOfWeek]} الساعة {departure:hh\\:mm}.");
    }

    public async Task<(bool Success, string Message)> SetLineActiveAsync(int lineId, bool isActive)
    {
        var line = await _dbContext.RouteLines.FindAsync(lineId);
        if (line == null)
        {
            return (false, "الخط غير موجود.");
        }

        line.IsActive = isActive;
        await _dbContext.SaveChangesAsync();
        return (true, isActive ? "أُعيد تفعيل الخط." : "أُوقف الخط مؤقتاً ولن يظهر للمسافرين.");
    }

    public async Task<(bool Success, string Message)> DeleteLineAsync(int lineId)
    {
        var line = await _dbContext.RouteLines
            .Include(l => l.Stops)
            .Include(l => l.Schedules)
            .FirstOrDefaultAsync(l => l.Id == lineId);
        if (line == null)
        {
            return (false, "الخط غير موجود.");
        }

        _dbContext.RouteLines.Remove(line); // مواقفه وجداوله تُحذف بالتتابع (Cascade)
        await _dbContext.SaveChangesAsync();
        return (true, "حُذف الخط ومواقفه وجداوله نهائياً.");
    }
}
