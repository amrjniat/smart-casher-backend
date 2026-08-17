namespace POS.DTOs
{
    public class UpdateSettingsRequest
    {
        public string? TimeZone { get; set; }
        public int? FiscalYearStartMonth { get; set; }
        public string? DateFormat { get; set; }

        public string? InvoicePrefix { get; set; }
        public int? InvoicePadding { get; set; }
        public string? InvoiceFooterMessage { get; set; }
        public int? DefaultPrintCopies { get; set; }
        public bool? ShowCompanyLogoOnInvoice { get; set; }

        public decimal? DefaultTaxRate { get; set; }
        public string? TaxCalculationMethod { get; set; }

        public int? DefaultLowStockAlert { get; set; }
        public bool? AutoUpdateStockAfterSale { get; set; }
        public bool? AutoUpdateStockAfterReturn { get; set; }
        public bool? EnableLowStockAlerts { get; set; }
        public bool? AllowNegativeStock { get; set; }
        public bool? UseBarcodeScannerInPOS { get; set; }

        public string? DefaultLanguage { get; set; }
        public string? AvailableLanguages { get; set; }

        public string? DiscountPrefix { get; set; }
        public string? DiscountSuffix { get; set; }
        public string? DiscountPosition { get; set; }
        public string? SystemName { get; set; }
        public bool? AutoPrintOnPayment { get; set; }
        public int? DecimalPlaces { get; set; }
        public string? CurrencySymbolPosition { get; set; }
        public bool? RoundDecimals { get; set; }
        public string? DefaultDirection { get; set; }
    }
}
