using AlMosafer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

/// <summary>
/// مساعد المسافر الذكي (P54): نقطة استفسار عامة بلا تسجيل دخول —
/// GET غير مبدّل للحالة (آمن من CSRF بطبيعته) ويعيد JSON للواجهة.
/// </summary>
public class AssistantController : Controller
{
    private readonly IAssistantService _assistant;

    public AssistantController(IAssistantService assistant)
    {
        _assistant = assistant;
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Ask(string q)
    {
        var reply = await _assistant.AskAsync(q ?? string.Empty);
        return Json(reply);
    }
}
