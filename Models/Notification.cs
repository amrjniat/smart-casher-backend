

using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class Notification : BaseEntity
    {
        public int UserId { get; set; }              
        public string Title { get; set; } = string.Empty; 
        public string Message { get; set; } = string.Empty; 
        public string? Type { get; set; }
        public string Module { get; set; } = "general";   // ← جديد: sales, inventory, clients, suppliers, invoices, general
        public string? RedirectUrl { get; set; }       
        public bool IsRead { get; set; } = false;       
        public DateTime? ReadAt { get; set; }           

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}




