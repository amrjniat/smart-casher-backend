namespace POS.Models
{
    public class AuditLog : BaseEntity
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? EntityName { get; set; }
        public int? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Browser { get; set; }
        public string? Device { get; set; }
        public string ActionType { get; set; } = string.Empty; // Create, Update, Delete, Login

        public virtual User User { get; set; } = null!;
    }
}