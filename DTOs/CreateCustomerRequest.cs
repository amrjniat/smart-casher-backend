namespace POS.DTOs
{
    public class CreateCustomerRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerCode { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? TaxNumber { get; set; }
        public decimal? OpeningBalance { get; set; }
        public int? LoyaltyPoints { get; set; }
        public string? Notes { get; set; }
    }
}
