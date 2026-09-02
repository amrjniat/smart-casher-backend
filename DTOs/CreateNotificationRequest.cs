
namespace POS.DTOs
{
    public class CreateNotificationRequest
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string Module { get; set; } = "general";
        public string? Source { get; set; }
        public int? EntityId { get; set; }
        public string? RedirectUrl { get; set; }
    }
}