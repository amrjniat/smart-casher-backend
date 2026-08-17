namespace POS.Models
{
    public class DiscountSetting : BaseEntity
    {
        public string DiscountName { get; set; } = string.Empty;
        public string? DiscountCode { get; set; }
        public string DiscountType { get; set; } = string.Empty; // Percentage, Fixed
        public decimal DiscountValue { get; set; } = 0;
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinPurchaseAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; } = 0;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ApplicableProducts { get; set; }
        public string? ApplicableCategories { get; set; }
        public string? Notes { get; set; }
    }
}