// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using POS.Data;
// using POS.DTOs;
// using POS.Models;
// using System.Security.Claims;

// namespace POS.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [Authorize]
//     public class NotificationsController : ControllerBase
//     {
//         private readonly AppDbContext _context;

//         public NotificationsController(AppDbContext context)
//         {
//             _context = context;
//         }

//         // ==================== 📋 1. جلب إشعارات المستخدم الحالي ====================
//         // GET: api/notifications
//         [HttpGet]
//         public async Task<IActionResult> GetMyNotifications(
//             [FromQuery] bool? unreadOnly = null,
//             [FromQuery] string? type = null,
//             [FromQuery] int page = 1,
//             [FromQuery] int pageSize = 20)
//         {
//             var userId = GetCurrentUserId();

//             var query = _context.Notifications
//                 .Where(n => n.UserId == userId && n.IsActive)
//                 .AsQueryable();

//             // فلتر الإشعارات غير المقروءة
//             if (unreadOnly.HasValue && unreadOnly.Value)
//                 query = query.Where(n => !n.IsRead);

//             // فلتر حسب النوع
//             if (!string.IsNullOrEmpty(type))
//                 query = query.Where(n => n.Type == type);

//             var totalItems = await query.CountAsync();

//             var notifications = await query
//                 .OrderByDescending(n => n.CreatedAt)
//                 .Skip((page - 1) * pageSize)
//                 .Take(pageSize)
//                 .Select(n => new
//                 {
//                     n.Id,
//                     n.Title,
//                     n.Message,
//                     n.Type,
//                     n.RedirectUrl,
//                     n.IsRead,
//                     n.CreatedAt,
//                     n.ReadAt
//                 })
//                 .ToListAsync();

//             return Ok(new
//             {
//                 totalItems,
//                 page,
//                 pageSize,
//                 totalPages = (int)Math.Ceiling((double)totalItems / pageSize),
//                 notifications
//             });
//         }

//         // ==================== 🔢 2. جلب عدد الإشعارات غير المقروءة ====================
//         // GET: api/notifications/unread-count
//         [HttpGet("unread-count")]
//         public async Task<IActionResult> GetUnreadCount()
//         {
//             var userId = GetCurrentUserId();

//             var count = await _context.Notifications
//                 .CountAsync(n => n.UserId == userId && !n.IsRead && n.IsActive);

//             return Ok(new { unreadCount = count });
//         }

//         // ==================== 👁️ 3. وضع علامة "مقروء" على إشعار معين ====================
//         // PUT: api/notifications/{id}/read
//         [HttpPut("{id}/read")]
//         public async Task<IActionResult> MarkAsRead(int id)
//         {
//             var userId = GetCurrentUserId();

//             var notification = await _context.Notifications
//                 .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

//             if (notification == null)
//                 return NotFound(new { message = "الإشعار غير موجود" });

//             notification.IsRead = true;
//             notification.ReadAt = DateTime.Now;
//             notification.UpdatedAt = DateTime.Now;

//             await _context.SaveChangesAsync();

//             return Ok(new { message = "تم وضع علامة مقروء", notification });
//         }

//         // ==================== ✅ 4. وضع علامة "مقروء" على جميع الإشعارات ====================
//         // PUT: api/notifications/read-all
//         [HttpPut("read-all")]
//         public async Task<IActionResult> MarkAllAsRead()
//         {
//             var userId = GetCurrentUserId();

//             var notifications = await _context.Notifications
//                 .Where(n => n.UserId == userId && !n.IsRead && n.IsActive)
//                 .ToListAsync();

//             foreach (var notification in notifications)
//             {
//                 notification.IsRead = true;
//                 notification.ReadAt = DateTime.Now;
//                 notification.UpdatedAt = DateTime.Now;
//             }

//             await _context.SaveChangesAsync();

//             return Ok(new { message = $"تم وضع علامة مقروء على {notifications.Count} إشعارات" });
//         }

//         // ==================== 🗑️ 5. حذف إشعار معين ====================
//         // DELETE: api/notifications/{id}
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteNotification(int id)
//         {
//             var userId = GetCurrentUserId();

//             var notification = await _context.Notifications
//                 .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

//             if (notification == null)
//                 return NotFound(new { message = "الإشعار غير موجود" });

//             // حذف ناعم
//             notification.IsActive = false;
//             notification.UpdatedAt = DateTime.Now;
//             await _context.SaveChangesAsync();

//             return Ok(new { message = "تم حذف الإشعار بنجاح" });
//         }

//         // ==================== 🗑️ 6. حذف جميع الإشعارات المقروءة ====================
//         // DELETE: api/notifications/clear-read
//         [HttpDelete("clear-read")]
//         public async Task<IActionResult> ClearReadNotifications()
//         {
//             var userId = GetCurrentUserId();

//             var notifications = await _context.Notifications
//                 .Where(n => n.UserId == userId && n.IsRead && n.IsActive)
//                 .ToListAsync();

//             foreach (var notification in notifications)
//             {
//                 notification.IsActive = false;
//                 notification.UpdatedAt = DateTime.Now;
//             }

//             await _context.SaveChangesAsync();

//             return Ok(new { message = $"تم حذف {notifications.Count} إشعارات مقروءة" });
//         }

//         // ==================== ➕ 7. إنشاء إشعار جديد (للاستخدام الداخلي أو للمشرف) ====================
//         // POST: api/notifications
//         [HttpPost]
//         [Authorize(Roles = "مدير النظام")]
//         public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
//         {
//             // التحقق من وجود المستخدم
//             var user = await _context.Users.FindAsync(request.UserId);
//             if (user == null)
//                 return NotFound(new { message = "المستخدم غير موجود" });

//             var notification = new Notification
//             {
//                 UserId = request.UserId,
//                 Title = request.Title,
//                 Message = request.Message,
//                 Type = request.Type,
//                 RedirectUrl = request.RedirectUrl,
//                 IsRead = false,
//                 IsActive = true,
//                 CreatedAt = DateTime.Now
//             };

//             _context.Notifications.Add(notification);
//             await _context.SaveChangesAsync();

//             return CreatedAtAction(nameof(GetMyNotifications), new { id = notification.Id }, notification);
//         }

//         // ==================== 🛠️ دوال مساعدة ====================

//         private int GetCurrentUserId()
//         {
//             var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//             return userId != null ? int.Parse(userId) : 0;
//         }
//     }


// }







using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.DTOs;
using POS.Models;
using System.Security.Claims;

namespace POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== 📋 1. جلب إشعارات المستخدم الحالي ====================
        // GET: api/notifications
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] bool? unreadOnly = null,
            [FromQuery] string? type = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();

            var query = _context.Notifications
                .Where(n => n.UserId == userId && n.IsActive)
                .AsQueryable();

            // فلتر الإشعارات غير المقروءة
            if (unreadOnly.HasValue && unreadOnly.Value)
                query = query.Where(n => !n.IsRead);

            // فلتر حسب النوع
            if (!string.IsNullOrEmpty(type))
                query = query.Where(n => n.Type == type);

            var totalItems = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.Module,
                    n.RedirectUrl,
                    n.IsRead,
                    n.CreatedAt,
                    n.ReadAt
                })
                .ToListAsync();

            return Ok(new
            {
                totalItems,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                notifications
            });
        }

        // ==================== 🔢 2. جلب عدد الإشعارات غير المقروءة ====================
        // GET: api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();

            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead && n.IsActive);

            return Ok(new { unreadCount = count });
        }

        // ==================== 👁️ 3. وضع علامة "مقروء" على إشعار معين ====================
        // PUT: api/notifications/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
                return NotFound(new { message = "الإشعار غير موجود" });

            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;
            notification.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم وضع علامة مقروء", notification });
        }

        // ==================== ✅ 4. وضع علامة "مقروء" على جميع الإشعارات ====================
        // PUT: api/notifications/read-all
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && n.IsActive)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                notification.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"تم وضع علامة مقروء على {notifications.Count} إشعارات" });
        }

        // ==================== 🗑️ 5. حذف إشعار معين ====================
        // DELETE: api/notifications/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = GetCurrentUserId();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
                return NotFound(new { message = "الإشعار غير موجود" });

            // حذف ناعم
            notification.IsActive = false;
            notification.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف الإشعار بنجاح" });
        }

        // ==================== 🗑️ 6. حذف جميع الإشعارات المقروءة ====================
        // DELETE: api/notifications/clear-read
        [HttpDelete("clear-read")]
        public async Task<IActionResult> ClearReadNotifications()
        {
            var userId = GetCurrentUserId();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsRead && n.IsActive)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsActive = false;
                notification.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"تم حذف {notifications.Count} إشعارات مقروءة" });
        }

        // ==================== ➕ 7. إنشاء إشعار جديد (للاستخدام الداخلي أو للمشرف) ====================
        // POST: api/notifications
        [HttpPost]
[Authorize(Roles = "Admin")]

        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            // التحقق من وجود المستخدم
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود" });

            var notification = new Notification
            {
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                Module = request.Module,
                RedirectUrl = request.RedirectUrl,
                IsRead = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyNotifications), new { id = notification.Id }, notification);
        }

        // ==================== 🛠️ دوال مساعدة ====================

        private int GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId != null ? int.Parse(userId) : 0;
        }
    }


}