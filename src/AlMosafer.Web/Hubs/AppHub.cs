using System.Security.Claims;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Web.Hubs;

/// <summary>
/// مركز «النبض الحي»: قناة WebSocket دائمة بين المتصفح والخادم.
/// كل اتصال يُقيَّد في مجموعة <c>user-{id}</c> من مطالبة NameIdentifier،
/// فيصل البث لصاحبه فقط (لا يُبث إشعار أو رسالة مستخدمٍ لآخر).
/// المصادقة: نفس كوكي الجلسة القائم — [Authorize] يرفض الزوّار.
/// </summary>
[Authorize]
public class AppHub : Hub
{
    private readonly AlMosaferDbContext _db;

    public AppHub(AlMosaferDbContext db)
    {
        _db = db;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// انضمام لمجموعة محادثة واحدة (صفحة التفاصيل) — بشرط العضوية الفعلية:
    /// السائق أو المسافر صاحبا الحجز فقط. غير ذلك = رفض صامت (IDOR).
    /// </summary>
    public async Task JoinConversation(int conversationId)
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId) || conversationId <= 0)
        {
            return;
        }

        var isMember = await _db.Conversations.AsNoTracking()
            .AnyAsync(c => c.Id == conversationId && (c.DriverId == userId || c.TravelerId == userId));

        if (isMember)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conv-{conversationId}");
        }
    }
}
