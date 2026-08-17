using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;
using System.Security.Claims;

namespace POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FinanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FinanceController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== 📊 إحصائيات عامة ====================

        // GET: api/finance/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] string period = "today")
        {
            var now = DateTime.Now;
            var startDate = period switch
            {
                "today" => now.Date,
                "week" => now.AddDays(-7),
                "month" => now.AddMonths(-1),
                _ => now.Date
            };

            // عدد الفواتير
            var invoicesCount = await _context.Invoices
                .Where(i => i.InvoiceDate >= startDate && i.Status != "ملغاة")
                .CountAsync();

            // مبيعات اليوم
            var totalSales = await _context.Invoices
                .Where(i => i.InvoiceDate >= startDate && i.Status != "ملغاة")
                .SumAsync(i => i.TotalAmount);

            // إجمالي التكاليف (من حركات الشراء)
            var totalCost = await _context.StockMovements
                .Where(m => m.MovementType == "إدخال" && m.CreatedAt >= startDate)
                .SumAsync(m => m.TotalPrice);

            // الربح الصافي
            var netProfit = totalSales - totalCost;

            // المواد منخفضة المخزون
            var lowStockItems = await _context.ProductWarehouses
                .Where(pw => pw.Quantity < pw.ReorderPoint && pw.Quantity > 0)
                .CountAsync();

            // عدد الفواتير الملغاة
            var cancelledInvoices = await _context.Invoices
                .Where(i => i.InvoiceDate >= startDate && i.Status == "ملغاة")
                .CountAsync();

            // الفواتير غير المدفوعة
            var unpaidInvoices = await _context.Invoices
                .Where(i => i.InvoiceDate >= startDate && i.Status == "غير مدفوعة")
                .CountAsync();

            return Ok(new
            {
                totalInvoices = invoicesCount,
                totalSales,
                totalCost,
                netProfit,
                lowStockItems,
                cancelledInvoices,
                unpaidInvoices,
                period
            });
        }

        // ==================== 🏆 المنتجات الأكثر مبيعاً ====================

        // GET: api/finance/top-products
        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts(
            [FromQuery] string period = "today",
            [FromQuery] int count = 5)
        {
            var now = DateTime.Now;
            var startDate = period switch
            {
                "today" => now.Date,
                "week" => now.AddDays(-7),
                "month" => now.AddMonths(-1),
                _ => now.Date
            };

            var topProducts = await _context.InvoiceItems
                .Include(ii => ii.Product)
                .Where(ii => ii.Invoice.InvoiceDate >= startDate && ii.Invoice.Status != "ملغاة")
                .GroupBy(ii => new { ii.ProductId, ii.Product.ProductName })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.ProductName,
                    TotalQuantity = g.Sum(ii => ii.Quantity),
                    TotalRevenue = g.Sum(ii => ii.TotalPrice),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(count)
                .ToListAsync();

            return Ok(topProducts);
        }

        // ==================== 📋 آخر الفواتير ====================

        // GET: api/finance/recent-invoices
        [HttpGet("recent-invoices")]
        public async Task<IActionResult> GetRecentInvoices([FromQuery] int count = 10)
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Branch)
                .OrderByDescending(i => i.InvoiceDate)
                .Take(count)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceNumber,
                    i.InvoiceDate,
                    i.TotalAmount,
                    i.Status,
                    CustomerName = i.Customer.CustomerName,
                    BranchName = i.Branch.BranchName,
                    Time = i.InvoiceDate.ToString("HH:mm"),
                    ItemsCount = i.InvoiceItems.Count
                })
                .ToListAsync();

            return Ok(invoices);
        }

        // ==================== 📊 إحصائيات الموظفين ====================

        // GET: api/finance/employee-stats
        [HttpGet("employee-stats")]
        public async Task<IActionResult> GetEmployeeStats([FromQuery] string period = "today")
        {
            var now = DateTime.Now;
            var startDate = period switch
            {
                "today" => now.Date,
                "week" => now.AddDays(-7),
                "month" => now.AddMonths(-1),
                _ => now.Date
            };

            var employees = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Username,
                    InvoicesCount = _context.Invoices
                        .Count(i => i.UserId == u.Id && i.InvoiceDate >= startDate && i.Status != "ملغاة"),
                    TotalSales = _context.Invoices
                        .Where(i => i.UserId == u.Id && i.InvoiceDate >= startDate && i.Status != "ملغاة")
                        .Sum(i => i.TotalAmount),
                    Role = u.Role.RoleName
                })
                .Where(u => u.InvoicesCount > 0)
                .OrderByDescending(u => u.TotalSales)
                .ToListAsync();

            return Ok(employees);
        }

        // ==================== 💳 المدفوعات ====================

        // GET: api/finance/payments
        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? methodId)
        {
            var query = _context.Payments
                .Include(p => p.Invoice)
                .Include(p => p.PaymentMethod)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(p => p.PaymentDate >= from.Value);

            if (to.HasValue)
                query = query.Where(p => p.PaymentDate <= to.Value);

            if (methodId.HasValue)
                query = query.Where(p => p.PaymentMethodId == methodId.Value);

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new
                {
                    p.Id,
                    p.Amount,
                    p.PaymentDate,
                    p.ReferenceNumber,
                    InvoiceNumber = p.Invoice.InvoiceNumber,
                    PaymentMethod = p.PaymentMethod.MethodName,
                    p.Notes
                })
                .ToListAsync();

            return Ok(payments);
        }

        // ==================== 📈 توزيع المبيعات ====================

        // GET: api/finance/sales-distribution
        [HttpGet("sales-distribution")]
        public async Task<IActionResult> GetSalesDistribution(
            [FromQuery] string period = "today")
        {
            var now = DateTime.Now;
            var startDate = period switch
            {
                "today" => now.Date,
                "week" => now.AddDays(-7),
                "month" => now.AddMonths(-1),
                _ => now.Date
            };

            var distribution = await _context.InvoiceItems
                .Include(ii => ii.Product)
                .ThenInclude(p => p.Category)
                .Where(ii => ii.Invoice.InvoiceDate >= startDate && ii.Invoice.Status != "ملغاة")
                .GroupBy(ii => new {
                    ii.Product.CategoryId,
                    CategoryName = ii.Product.Category != null ? ii.Product.Category.CategoryName : "بدون تصنيف"
                })
                .Select(g => new
                {
                    g.Key.CategoryId,
                    g.Key.CategoryName,
                    TotalSales = g.Sum(ii => ii.TotalPrice),
                    TotalQuantity = g.Sum(ii => ii.Quantity),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSales)
                .ToListAsync();

            return Ok(distribution);
        }

        // ==================== 🛠️ دوال مساعدة ====================

        private int GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId != null ? int.Parse(userId) : 0;
        }
    }
}