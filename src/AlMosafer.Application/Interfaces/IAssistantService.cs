using AlMosafer.Application.DTOs.Assistant;

namespace AlMosafer.Application.Interfaces;

/// <summary>
/// مساعد المسافر الذكي: يجيب أسئلة العملاء بالعربية فوراً بلا مفاتيح خارجية —
/// محرك نوايا قاعدي (Rule-Based) واعٍ ببيانات المنصة الحية، وجاهز معمارياً لترقية إلى LLM لاحقاً.
/// </summary>
public interface IAssistantService
{
    Task<AssistantReplyDto> AskAsync(string message);
}
