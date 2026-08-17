namespace POS.DTOs
{
    public class RegisterRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? StoreName { get; set; } // ✅ الحقل الجديد
        public int RoleId { get; set; } = 1;
    }
}