namespace AlMosafer.Domain.Entities;

public class RouteLine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<LineStop> Stops { get; set; } = new();
    public List<LineSchedule> Schedules { get; set; } = new();
}
