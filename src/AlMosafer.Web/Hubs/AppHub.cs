using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AlMosafer.Web.Hubs;

/// <summary>
/// مركز «النبض الحي» (P40): قناة WebSocket دائمة بين المتصفح والخادم.
/// كل اتصال يُقيَّد في مجموعة <c>user-{id}</c> من مطالبة NameIdentifier،
/// فيصل البث لصاحبه فقط (لا يُبث إشعار مستخدمٍ لآخر).
/// المصادقة: نفس كوكي الجلسة القائم — [Authorize] يرفض الزوّار.
/// </summary>
[Authorize]
public class AppHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        await base.OnConnectedAsync();
    }
}
