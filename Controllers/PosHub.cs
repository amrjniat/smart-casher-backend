using Microsoft.AspNetCore.SignalR;

namespace POS.Hubs
{
    public class PosHub : Hub
    {
        // يمكن ترك الكلاس فارغاً لأننا نرسل الإشعارات مباشرة من الـ Controllers عبر IHubContext
    }
}