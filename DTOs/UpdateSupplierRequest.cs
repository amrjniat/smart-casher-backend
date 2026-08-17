namespace POS.DTOs
{
    public class UpdateSupplierRequest
    {
        public string? SupplierName { get; set; }
        public string? SupplierCode { get; set; }
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? TaxNumber { get; set; }
        public string? CommercialRegister { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string? Notes { get; set; }
        public bool? IsActive { get; set; }
        public string? Status { get; set; }
    }
}
