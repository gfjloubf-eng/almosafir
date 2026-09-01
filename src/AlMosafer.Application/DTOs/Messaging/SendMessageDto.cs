using System.ComponentModel.DataAnnotations;

namespace AlMosafer.Application.DTOs.Messaging;

public class SendMessageDto
{
    [Required]
    public int ConversationId { get; set; }

    [Required(ErrorMessage = "نص الرسالة مطلوب")]
    [StringLength(1000, ErrorMessage = "الرسالة يجب أن تكون أقل من 1000 حرف")]
    public string Content { get; set; } = string.Empty;
}
