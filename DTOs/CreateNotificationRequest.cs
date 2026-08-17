// namespace POS.DTOs
// {
//     public class CreateNotificationRequest
//     {
//         public int UserId { get; set; }
//         public string Title { get; set; } = string.Empty;
//         public string Message { get; set; } = string.Empty;
//         public string? Type { get; set; }
//         public string? RedirectUrl { get; set; }
//     }
// }


namespace POS.DTOs
{
    public class CreateNotificationRequest
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string Module { get; set; } = "general";   // ← جديد
        public string? RedirectUrl { get; set; }
    }
}