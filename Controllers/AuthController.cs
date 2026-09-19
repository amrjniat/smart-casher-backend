using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.DTOs;
using POS.Models;
using POS.Services;

namespace POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly PasswordService _passwordService;

        public AuthController(AppDbContext context, JwtService jwtService, PasswordService passwordService)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordService = passwordService;
        }

        // 📌 تسجيل الدخول
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                return Unauthorized(new { message = "اسم المستخدم غير صحيح" });

            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "كلمة المرور غير صحيحة" });

            // ✅ التحقق من أن الحساب مفعّل قبل السماح بتسجيل الدخول
            if (!user.IsActive)
                return Unauthorized(new { message = "حسابك تم تعطيله، يرجى مراجعة الإدارة" });

            if (!user.BranchId.HasValue)
                return BadRequest(new { message = "الحساب غير مرتبط بفرع صالح. يرجى ربط المستخدم بفرع أولاً." });

            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == user.BranchId.Value && b.IsActive);
            if (branch == null)
                return NotFound(new { message = "الفرع المرتبط بالمستخدم غير موجود أو غير مفعل." });

            var warehouse = await EnsureDefaultWarehouseAsync(branch.Id);
            if (warehouse == null)
                return NotFound(new { message = "تعذر إنشاء أو العثور على مستودع نشط للفرع المرتبط بالمستخدم." });

            var warehouseId = (int?)warehouse.Id;

            user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user, user.Role?.RoleName ?? "", warehouseId);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Username,
                    user.Email,
                    user.Phone,
                    Role = user.Role?.RoleName,
                    user.BranchId,
                    WarehouseId = warehouseId
                }
            });
        }

        private async Task<Warehouse?> EnsureDefaultWarehouseAsync(int branchId)
        {
            var warehouse = await _context.Warehouses
                .Where(w => w.IsActive && w.BranchId == branchId)
                .OrderByDescending(w => w.IsMainWarehouse)
                .ThenBy(w => w.Id)
                .FirstOrDefaultAsync();

            if (warehouse != null)
                return warehouse;

            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null || !branch.IsActive)
                return null;

            warehouse = new Warehouse
            {
                WarehouseName = $"المستودع الرئيسي - {branch.BranchName}",
                WarehouseCode = $"BR-{branchId}-MAIN",
                IsMainWarehouse = true,
                BranchId = branchId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }

        // 📌 إنشاء حساب جديد
        [HttpPost("register")]
        [Authorize(Roles = "Admin,مدير النظام,Administrator")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
                return BadRequest(new { message = "اسم المستخدم موجود مسبقاً" });

            var user = new User
            {
                FullName = request.FullName,
                Username = request.Username,
                PasswordHash = _passwordService.HashPassword(request.Password),
                Email = request.Email,
                Phone = request.Phone,
                RoleId = request.RoleId,
                BranchId = request.BranchId,
                //StoreName = request.StoreName,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            if (!request.BranchId.HasValue)
                return BadRequest(new { message = "يجب تحديد فرع صالح للحساب." });

            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == request.BranchId.Value && b.IsActive);
            if (branch == null)
                return NotFound(new { message = "الفرع المحدد غير موجود أو غير مفعل." });

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await EnsureDefaultWarehouseAsync(branch.Id);

            return Ok(new { message = "تم إنشاء الحساب بنجاح" });
        }

        // 📌 الحصول على معلومات المستخدم الحالي
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
                return NotFound();

            var warehouseId = user.BranchId.HasValue
                ? await _context.Warehouses
                    .Where(w => w.IsActive && w.BranchId == user.BranchId)
                    .OrderByDescending(w => w.IsMainWarehouse)
                    .ThenBy(w => w.Id)
                    .Select(w => (int?)w.Id)
                    .FirstOrDefaultAsync()
                : null;

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Username,
                user.Email,
                user.Phone,
                Role = user.Role?.RoleName,
                user.BranchId,
                WarehouseId = warehouseId
            });
        }
    }
}