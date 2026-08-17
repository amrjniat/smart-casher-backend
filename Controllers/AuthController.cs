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

user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

           var token = _jwtService.GenerateToken(user, user.Role?.RoleName ?? "");

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
                    Role = user.Role?.RoleName
                }
            });
        }

        // 📌 إنشاء حساب جديد
        [HttpPost("register")]
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
                //StoreName = request.StoreName,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إنشاء الحساب بنجاح" });
        }

        // 📌 الحصول على معلومات المستخدم الحالي
        [HttpGet("me")]
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

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Username,
                user.Email,
                user.Phone,
                Role = user.Role?.RoleName
            });
        }
    }
}