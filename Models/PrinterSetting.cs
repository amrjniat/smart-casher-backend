namespace POS.Models
{
    public class PrinterSetting : BaseEntity
    {
        public string PrinterName { get; set; } = string.Empty;
        public string PrinterType { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public int? Port { get; set; }
        public string? PrinterModel { get; set; }
        public int? PaperWidth { get; set; }
        public int? PaperHeight { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsThermal { get; set; } = true;
        public int? CopyCount { get; set; } = 1;

        public int? BranchId { get; set; }

        public virtual Branch? Branch { get; set; }
    }
}