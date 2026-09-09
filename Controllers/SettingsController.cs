using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.DTOs;
using POS.Models;

namespace POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,مدير النظام,Administrator")]
    public class SettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SettingsController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== 📊 1. جلب جميع الإعدادات ====================
        // GET: api/settings
        [HttpGet]
        public async Task<IActionResult> GetAllSettings()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SystemSettings
                {
                    TimeZone = "Asia/Damascus",
                    FiscalYearStartMonth = 1,
                    DateFormat = "DD/MM/YYYY",
                    InvoicePrefix = "INV-",
                    InvoicePadding = 6,
                    InvoiceFooterMessage = "شكراً لتسوقكم معاً",
                    DefaultPrintCopies = 1,
                    ShowCompanyLogoOnInvoice = true,
                    DefaultTaxRate = 0,
                    TaxCalculationMethod = "AfterDiscount",
                    DefaultLowStockAlert = 10,
                    AutoUpdateStockAfterSale = true,
                    AutoUpdateStockAfterReturn = true,
                    EnableLowStockAlerts = true,
                    AllowNegativeStock = false,
                    UseBarcodeScannerInPOS = true,
                    DefaultLanguage = "ar",
                    AvailableLanguages = "ar,en,fr",
                    DiscountPrefix = "",
                    DiscountSuffix = "",
                    DiscountPosition = "AfterNumber",
                    IsActive = true,
                    SystemName = "SmartCheckout POS",
                    AutoPrintOnPayment = true,
                    DecimalPlaces = 2,
                    CurrencySymbolPosition = "AfterNumber",
                    RoundDecimals = false,
                    DefaultDirection = "rtl",
                    CreatedAt = DateTime.Now
                };

                _context.SystemSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return Ok(settings);
        }

        // ==================== 📊 2. تحديث الإعدادات ====================
        // PUT: api/settings
        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SystemSettings();
                _context.SystemSettings.Add(settings);
            }

            // تحديث الإعدادات
            settings.TimeZone = request.TimeZone ?? settings.TimeZone;
            settings.FiscalYearStartMonth = request.FiscalYearStartMonth ?? settings.FiscalYearStartMonth;
            settings.DateFormat = request.DateFormat ?? settings.DateFormat;

            settings.InvoicePrefix = request.InvoicePrefix ?? settings.InvoicePrefix;
            settings.InvoicePadding = request.InvoicePadding ?? settings.InvoicePadding;
            settings.InvoiceFooterMessage = request.InvoiceFooterMessage ?? settings.InvoiceFooterMessage;
            settings.DefaultPrintCopies = request.DefaultPrintCopies ?? settings.DefaultPrintCopies;
            settings.ShowCompanyLogoOnInvoice = request.ShowCompanyLogoOnInvoice ?? settings.ShowCompanyLogoOnInvoice;

            settings.DefaultTaxRate = request.DefaultTaxRate ?? settings.DefaultTaxRate;
            settings.TaxCalculationMethod = request.TaxCalculationMethod ?? settings.TaxCalculationMethod;

            settings.DefaultLowStockAlert = request.DefaultLowStockAlert ?? settings.DefaultLowStockAlert;
            settings.AutoUpdateStockAfterSale = request.AutoUpdateStockAfterSale ?? settings.AutoUpdateStockAfterSale;
            settings.AutoUpdateStockAfterReturn = request.AutoUpdateStockAfterReturn ?? settings.AutoUpdateStockAfterReturn;
            settings.EnableLowStockAlerts = request.EnableLowStockAlerts ?? settings.EnableLowStockAlerts;
            settings.AllowNegativeStock = request.AllowNegativeStock ?? settings.AllowNegativeStock;
            settings.UseBarcodeScannerInPOS = request.UseBarcodeScannerInPOS ?? settings.UseBarcodeScannerInPOS;

            settings.DefaultLanguage = request.DefaultLanguage ?? settings.DefaultLanguage;
            settings.AvailableLanguages = request.AvailableLanguages ?? settings.AvailableLanguages;

            settings.DiscountPrefix = request.DiscountPrefix ?? settings.DiscountPrefix;
            settings.DiscountSuffix = request.DiscountSuffix ?? settings.DiscountSuffix;
            settings.DiscountPosition = request.DiscountPosition ?? settings.DiscountPosition;
            settings.SystemName = request.SystemName ?? settings.SystemName;
            settings.AutoPrintOnPayment = request.AutoPrintOnPayment ?? settings.AutoPrintOnPayment;
            settings.DecimalPlaces = request.DecimalPlaces ?? settings.DecimalPlaces;
            settings.CurrencySymbolPosition = request.CurrencySymbolPosition ?? settings.CurrencySymbolPosition;
            settings.RoundDecimals = request.RoundDecimals ?? settings.RoundDecimals;
            settings.DefaultDirection = request.DefaultDirection ?? settings.DefaultDirection;
            settings.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تحديث الإعدادات بنجاح", settings });
        }

        // ==================== 🏢 3. معلومات الشركة ====================
        // GET: api/settings/company
        [HttpGet("company")]
        public async Task<IActionResult> GetCompanyInfo()
        {
            var company = await _context.CompanyInfos.FirstOrDefaultAsync();

            if (company == null)
            {
                company = new CompanyInfo
                {
                    CompanyName = "سوبر ماركت الأمل",
                    CompanyNameAr = "سوبر ماركت الأمل",
                    Address = "سوريا",
                    Phone = "0991234567",
                    Email = "info@supermarket.com",
                    TaxNumber = "123456789",
                    CurrencySymbol = "ل.س",
                    CurrencyCode = "SYP",
                    FooterNote = "شكراً لتسوقكم معاً",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                _context.CompanyInfos.Add(company);
                await _context.SaveChangesAsync();
            }

            return Ok(company);
        }

        // ==================== 🏢 4. تحديث معلومات الشركة ====================
        // PUT: api/settings/company
        [HttpPut("company")]
        public async Task<IActionResult> UpdateCompanyInfo([FromBody] UpdateCompanyInfoRequest request)
        {
          
            var company = await _context.CompanyInfos.FirstOrDefaultAsync();

            if (company == null)
            {
                company = new CompanyInfo();
                _context.CompanyInfos.Add(company);
            }

            company.CompanyName = request.CompanyName ?? company.CompanyName;
            company.CompanyNameAr = request.CompanyNameAr ?? company.CompanyNameAr;
            company.Address = request.Address ?? company.Address;
            company.Phone = request.Phone ?? company.Phone;
            company.Mobile = request.Mobile ?? company.Mobile;
            company.Email = request.Email ?? company.Email;
            company.Website = request.Website ?? company.Website;
            company.TaxNumber = request.TaxNumber ?? company.TaxNumber;
            company.CommercialRegister = request.CommercialRegister ?? company.CommercialRegister;
            company.FooterNote = request.FooterNote ?? company.FooterNote;
            company.InvoiceFooter = request.InvoiceFooter ?? company.InvoiceFooter;
            company.CurrencySymbol = request.CurrencySymbol ?? company.CurrencySymbol;
            company.CurrencyCode = request.CurrencyCode ?? company.CurrencyCode;
            company.TimeZone = request.TimeZone ?? company.TimeZone;
            company.DateFormat = request.DateFormat ?? company.DateFormat;
            company.LogoPath = request.LogoPath ?? company.LogoPath;
            company.HeaderImagePath = request.HeaderImagePath ?? company.HeaderImagePath;
            company.BranchName = request.BranchName ?? company.BranchName;
            company.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تحديث معلومات الشركة", company });
        }

        // ==================== 🖼️ 5. رفع شعار الشركة ====================
        // POST: api/settings/logo
        [HttpPost("logo")]
        public async Task<IActionResult> UploadLogo([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "الرجاء اختيار ملف" });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "الملف غير مدعوم" });

            if (file.Length > 2 * 1024 * 1024)
                return BadRequest(new { message = "حجم الملف كبير جداً. الحد الأقصى 2 ميجابايت" });

            var fileName = $"logo_{DateTime.Now:yyyyMMddHHmmss}{extension}";
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logos");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var company = await _context.CompanyInfos.FirstOrDefaultAsync();
            if (company != null)
            {
                company.LogoPath = $"/images/logos/{fileName}";
                company.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "تم رفع الشعار بنجاح",
                logoPath = $"/images/logos/{fileName}"
            });
        }

        // ==================== 🗑️ 6. حذف الشعار ====================
        // DELETE: api/settings/logo
        [HttpDelete("logo")]
        public async Task<IActionResult> DeleteLogo()
        {
            var company = await _context.CompanyInfos.FirstOrDefaultAsync();

            if (company == null || string.IsNullOrEmpty(company.LogoPath))
                return BadRequest(new { message = "لا يوجد شعار لحذفه" });

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", company.LogoPath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            company.LogoPath = null;
            company.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف الشعار بنجاح" });
        }
    }

  
    
}