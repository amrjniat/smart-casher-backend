namespace POS.Models
{
    public class SystemSettings : BaseEntity
    {
        // ===== إعدادات النظام العامة =====
        public string? TimeZone { get; set; } = "Asia/Damascus";
        public int? FiscalYearStartMonth { get; set; } = 1; // يناير
        public string? DateFormat { get; set; } = "DD/MM/YYYY";
        public string? SystemName { get; set; } = "SmartCheckout POS";

        // ===== إعدادات الفواتير =====
        public string? InvoicePrefix { get; set; } = "INV-";
        public int? InvoicePadding { get; set; } = 6;
        public string? InvoiceFooterMessage { get; set; } = "شكراً لتسوقكم معاً";
        public int? DefaultPrintCopies { get; set; } = 1;
        public bool ShowCompanyLogoOnInvoice { get; set; } = true;
        public bool AutoPrintOnPayment { get; set; } = true;
        // ===== إعدادات الضرائب =====
        public decimal? DefaultTaxRate { get; set; } = 0;
        public string? TaxCalculationMethod { get; set; } = "AfterDiscount"; // AfterDiscount / BeforeDiscount
        public int? DecimalPlaces { get; set; } = 2;    
        public string? CurrencySymbolPosition { get; set; } = "AfterNumber";
        public bool RoundDecimals { get; set; } = false;
        // ===== إعدادات المخزون =====
        public int? DefaultLowStockAlert { get; set; } = 10;
        public bool AutoUpdateStockAfterSale { get; set; } = true;
        public bool AutoUpdateStockAfterReturn { get; set; } = true;
        public bool EnableLowStockAlerts { get; set; } = true;
        public bool AllowNegativeStock { get; set; } = false;
        public bool UseBarcodeScannerInPOS { get; set; } = true;

        // ===== إعدادات اللغة =====
        public string? DefaultLanguage { get; set; } = "ar";
        public string? AvailableLanguages { get; set; } = "ar,en,fr";
        public string? DefaultDirection { get; set; } = "rtl";
        // ===== إعدادات الخصم =====
        public string? DiscountPrefix { get; set; } = "";
        public string? DiscountSuffix { get; set; } = "";
        public string? DiscountPosition { get; set; } = "AfterNumber"; // AfterNumber / BeforeNumber
    }
}