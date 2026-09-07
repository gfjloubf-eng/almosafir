namespace AlMosafer.Application.DTOs.Assistant;

/// <summary>
/// ردّ مساعد المسافر الذكي: إجابة نصية + اقتراحات أسئلة سريعة تظهر كأزرار.
/// </summary>
public class AssistantReplyDto
{
    public string Answer { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
}
