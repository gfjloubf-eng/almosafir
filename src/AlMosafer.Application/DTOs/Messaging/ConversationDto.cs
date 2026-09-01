namespace AlMosafer.Application.DTOs.Messaging;

public class ConversationDto
{
    public int ConversationId { get; set; }
    public int BookingId { get; set; }
    public int TripId { get; set; }
    public string TripRoute { get; set; } = string.Empty;
    public int OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string OtherUserRole { get; set; } = string.Empty;
    public string? LastMessage { get; set; }
    public DateTime LastMessageAt { get; set; }
}
