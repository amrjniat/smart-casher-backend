using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using POS.Controllers;
using POS.Data;
using POS.DTOs;
using POS.Hubs;
using POS.Models;

namespace POS.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<PosHub> _hubContext;

        public NotificationService(AppDbContext context, IHubContext<PosHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<NotificationDto> CreateAsync(CreateNotificationRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                throw new InvalidOperationException("المستخدم غير موجود");

            var notification = new Notification
            {
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type ?? "system",
                Module = request.Module,
                RedirectUrl = request.RedirectUrl,
                IsRead = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var dto = new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Module = notification.Module,
                Source = request.Source,
                EntityId = request.EntityId,
                RedirectUrl = notification.RedirectUrl,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt
            };

            await _hubContext.Clients.User(request.UserId.ToString())
                .SendAsync("ReceiveNotification", dto);

            await _hubContext.Clients.User(request.UserId.ToString())
                .SendAsync("UnreadCountUpdated", await GetUnreadCountAsync(request.UserId));

            return dto;
        }

        public async Task<List<NotificationDto>> GetByUserAsync(int userId, bool unreadOnly = false)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId && n.IsActive)
                .AsQueryable();

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    Module = n.Module,
                    Source = n.Source,
                    EntityId = n.EntityId,
                    RedirectUrl = n.RedirectUrl,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead && n.IsActive);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && n.IsActive);

            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;
            notification.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("NotificationRead", new { notificationId, userId });

            return true;
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && n.IsActive)
                .ToListAsync();

            foreach (var item in notifications)
            {
                item.IsRead = true;
                item.ReadAt = DateTime.Now;
                item.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("UnreadCountUpdated", 0);

            return notifications.Count;
        }

        public async Task<bool> DeleteAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && n.IsActive);

            if (notification == null)
                return false;

            notification.IsActive = false;
            notification.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("NotificationDeleted", new { notificationId, userId });

            return true;
        }
    }
}
