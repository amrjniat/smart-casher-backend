namespace POS.Models
{
    public class TaxSetting : BaseEntity
    {
        public string TaxName { get; set; } = string.Empty;
        public string? TaxCode { get; set; }
        public decimal TaxRate { get; set; } = 0;
        public bool IsDefault { get; set; } = false;
        public bool IsInclusive { get; set; } = false;
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
    }
}