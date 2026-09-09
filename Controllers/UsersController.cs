using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.DTOs;
using POS.Models;
using POS.Services;

namespace POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,مدير النظام,Administrator")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;

        public UsersController(AppDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        // ==================== 📊 1. إحصائيات الموظفين ====================
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalEmployees = await _context.Users.CountAsync();
            var activeEmployees = await _context.Users.CountAsync(u => u.IsActive);
            var disabledEmployees = totalEmployees - activeEmployees;

            var roleBreakdown = await _context.Users
                .Include(u => u.Role)
                .GroupBy(u => new { u.RoleId, u.Role.RoleName })
                .Select(g => new RoleCountDto
                {
                    RoleId = g.Key.RoleId,
                    RoleName = g.Key.RoleName,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(new UserStatsDto
            {
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                DisabledEmployees = disabledEmployees,
                RoleBreakdown = roleBreakdown
            });
        }

        // ==================== 📋 2. جلب قائمة الموظفين ====================
        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search = null,
            [FromQuery] int? roleId = null,
            [FromQuery] int? branchId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Branch)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(searchLower) ||
                    u.Username.ToLower().Contains(searchLower) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchLower)));
            }

            if (roleId.HasValue)
                query = query.Where(u => u.RoleId == roleId.Value);

            if (branchId.HasValue)
                query = query.Where(u => u.BranchId == branchId.Value);

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            var totalItems = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListItemDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    RoleId = u.RoleId,
                    RoleName = u.Role.RoleName,
                    BranchId = u.BranchId,
                    BranchName = u.Branch != null ? u.Branch.BranchName : null,
                    IsActive = u.IsActive,
                    IsLocked = u.IsLocked,
                    LastLogin = u.LastLogin,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                totalItems,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                users
            });
        }

        // ==================== 👤 3. جلب موظف معين (مع الصلاحيات) ====================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "الموظف غير موجود" });

            var dto = new UserDetailDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                RoleId = user.RoleId,
                RoleName = user.Role.RoleName,
                BranchId = user.BranchId,
                BranchName = user.Branch?.BranchName,
                IsActive = user.IsActive,
                IsLocked = user.IsLocked,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                Permissions = ParsePermissions(user.Role.Permissions)
            };

            return Ok(dto);
        }

        // ==================== ➕ 4. إضافة موظف جديد ====================
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
                return BadRequest(new { message = "اسم المستخدم موجود مسبقاً" });

            var roleExists = await _context.Roles.AnyAsync(r => r.Id == request.RoleId);
            if (!roleExists)
                return BadRequest(new { message = "الدور المحدد غير موجود" });

            var user = new User
            {
                FullName = request.FullName,
                Username = request.Username,
                PasswordHash = _passwordService.HashPassword(request.Password),
                Email = request.Email,
                Phone = request.Phone,
                RoleId = request.RoleId,
                BranchId = request.BranchId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new { message = "تم إنشاء الموظف بنجاح", userId = user.Id });
        }

        // ==================== ✏️ 5. تحديث بيانات موظف ====================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "الموظف غير موجود" });

            if (request.RoleId.HasValue)
            {
                var roleExists = await _context.Roles.AnyAsync(r => r.Id == request.RoleId.Value);
                if (!roleExists)
                    return BadRequest(new { message = "الدور المحدد غير موجود" });
                user.RoleId = request.RoleId.Value;
            }

            user.FullName = request.FullName ?? user.FullName;
            user.Email = request.Email ?? user.Email;
            user.Phone = request.Phone ?? user.Phone;
            user.BranchId = request.BranchId ?? user.BranchId;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تحديث بيانات الموظف" });
        }

        // ==================== 🔒 6. تفعيل / تعطيل موظف ====================
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "الموظف غير موجود" });

            user.IsActive = request.IsActive;

            if (request.IsActive)
            {
                user.IsLocked = false;
                user.FailedLoginAttempts = 0;
            }

            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = request.IsActive ? "تم تفعيل الحساب" : "تم تعطيل الحساب" });
        }

        // ==================== 🔑 7. إعادة تعيين كلمة المرور ====================
        [HttpPut("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "الموظف غير موجود" });

            user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تغيير كلمة المرور بنجاح" });
        }

        // ==================== 🗑️ 8. حذف موظف (Soft Delete) ====================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "الموظف غير موجود" });

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != null && int.Parse(currentUserId) == id)
                return BadRequest(new { message = "لا يمكنك حذف حسابك الخاص" });

            user.IsActive = false;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف الموظف" });
        }

        // ==================== 🛠️ دوال مساعدة ====================
        private List<string> ParsePermissions(string? permissions)
        {
            if (string.IsNullOrWhiteSpace(permissions))
                return new List<string>();

            try
            {
                if (permissions.TrimStart().StartsWith("["))
                    return System.Text.Json.JsonSerializer.Deserialize<List<string>>(permissions) ?? new();
            }
            catch { /* fallback below */ }

            return permissions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }
    }
}