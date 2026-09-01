using AlMosafer.Domain.Enums;

namespace AlMosafer.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public NotificationType Type { get; set; } = NotificationType.Booking;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User User { get; set; } = null!;
}
