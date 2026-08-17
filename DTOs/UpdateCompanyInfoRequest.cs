namespace POS.DTOs
{
    public class UpdateCompanyInfoRequest
    {
        public string? CompanyName { get; set; }
        public string? CompanyNameAr { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? TaxNumber { get; set; }
        public string? CommercialRegister { get; set; }
        public string? FooterNote { get; set; }
        public string? InvoiceFooter { get; set; }
        public string? CurrencySymbol { get; set; }
        public string? CurrencyCode { get; set; }
        public string? TimeZone { get; set; }
        public string? DateFormat { get; set; }
        public string? LogoPath { get; set; }
        public string? HeaderImagePath { get; set; }
        public string? BranchName { get; set; }
    }
}
