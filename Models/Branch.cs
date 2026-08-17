namespace POS.Models
{
    public class Branch : BaseEntity
    {
        public string BranchName { get; set; } = string.Empty;
        public string? BranchCode { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ManagerName { get; set; }
        public TimeOnly? OpenTime { get; set; }
        public TimeOnly? CloseTime { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
        public virtual ICollection<PrinterSetting> PrinterSettings { get; set; } = new List<PrinterSetting>();
    }
}