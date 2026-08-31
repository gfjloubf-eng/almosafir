using System.Security.Claims;
using AlMosafer.Application.DTOs.Messaging;
using AlMosafer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

[Authorize]
public class ConversationsController : Controller
{
    private readonly IConversationService _conversationService;
    private readonly IMessageService _messageService;

    public ConversationsController(IConversationService conversationService, IMessageService messageService)
    {
        _conversationService = conversationService;
        _messageService = messageService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var conversations = await _conversationService.GetUserConversationsAsync(userId);
        return View(conversations);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = GetCurrentUserId();
        var conversation = await _conversationService.GetConversationByIdAsync(userId, id);

        if (conversation == null)
        {
            return Forbid(); // IDOR Protection
        }

        var messages = await _messageService.GetConversationMessagesAsync(userId, id);
        ViewData["Conversation"] = conversation;
        ViewData["Messages"] = messages;

        return View(new SendMessageDto { ConversationId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(SendMessageDto dto)
    {
        var userId = GetCurrentUserId();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "محتوى الرسالة غير صحيح.";
            return RedirectToAction(nameof(Details), new { id = dto.ConversationId });
        }

        var result = await _messageService.SendMessageAsync(userId, dto);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Details), new { id = dto.ConversationId });
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}
