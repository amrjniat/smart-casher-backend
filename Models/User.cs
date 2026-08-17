namespace POS.Models
{
    public class User : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? StoreName { get; set; } // ✅ الحقل الجديد
        public string? Address { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsLocked { get; set; } = false;
        public int FailedLoginAttempts { get; set; } = 0;

        public int RoleId { get; set; }
        public int? BranchId { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual Branch? Branch { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}