using Microsoft.AspNetCore.SignalR;

namespace POS.Hubs
{
    public class PosHub : Hub
    {
        public async Task SendNotificationToUser(int userId, object notification)
        {
            await Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
        }

        public async Task UpdateUnreadCount(int userId, int count)
        {
            await Clients.User(userId.ToString()).SendAsync("UnreadCountUpdated", count);
        }
    }
}