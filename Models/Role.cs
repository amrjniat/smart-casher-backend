namespace POS.Models
{
    public class Role : BaseEntity
    {
        public string RoleName { get; set; } = string.Empty;
        public string? Permissions { get; set; } 
        public string? Description { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}