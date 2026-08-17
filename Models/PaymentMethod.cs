namespace POS.Models
{
    public class PaymentMethod : BaseEntity
    {
        public string MethodName { get; set; } = string.Empty;
        public string? PaymentType { get; set; } 
        public string? IconPath { get; set; }
        public bool IsDefault { get; set; } = false;
        public decimal? ProcessingFee { get; set; }
        public string? Notes { get; set; }
    }
}