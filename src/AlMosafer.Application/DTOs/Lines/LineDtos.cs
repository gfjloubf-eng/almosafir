namespace AlMosafer.Application.DTOs.Lines;

public class LineSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int StopsCount { get; set; }
    public int SchedulesCount { get; set; }
}

public class LineStopDto
{
    public string Name { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}

public class LineScheduleDto
{
    public string DayName { get; set; } = string.Empty;
    public string TimeText { get; set; } = string.Empty;
}

public class LineDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<LineStopDto> Stops { get; set; } = new();
    public List<LineScheduleDto> Schedules { get; set; } = new();
}
