using Microsoft.AspNetCore.SignalR;

namespace AlMosafer.Web.Hubs;

/// <summary>
/// مركز «البث العام» (P50 «عصر التطوير»): قناة WebSocket مفتوحة بلا تسجيل دخول
/// تبث اللحظات الحية العامة — المقاعد المتبقية في الرحلة ولوحة الانطلاق.
/// لا ينقل أي بيانات شخصية: فقط أحداث عامة (رقم المقاعد المتبقية، انطلاق رحلة).
/// المنطق الحساس (حجوزات/إشعارات/محادثات) يبقى محروساً في <see cref="AppHub"/>.
/// </summary>
public class LiveHub : Hub
{
    /// <summary>متابعة رحلة واحدة (صفحة التفاصيل) لمشاهدة مقاعدها وهي تتناقص لحظياً.</summary>
    public Task WatchTrip(int tripId)
    {
        if (tripId <= 0)
        {
            return Task.CompletedTask;
        }

        return Groups.AddToGroupAsync(Context.ConnectionId, $"trip-{tripId}");
    }

    /// <summary>الانضمام إلى مجموعة لوحة الانطلاق العامة لاستقبال لحظات الانطلاق.</summary>
    public Task WatchBoard()
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, "board");
    }
}
