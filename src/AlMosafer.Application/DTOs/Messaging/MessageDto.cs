namespace AlMosafer.Application.DTOs.Messaging;

public class MessageDto
{
    public int MessageId { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsMine { get; set; }
}
