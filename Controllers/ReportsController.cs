// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using POS.Data;

// namespace POS.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [Authorize]
//     public class ReportsController : ControllerBase
//     {
//         private readonly AppDbContext _context;

//         public ReportsController(AppDbContext context)
//         {
//             _context = context;
//         }

//         // ==================== 📊 تقرير المبيعات ====================

//         // GET: api/reports/sales
//         [HttpGet("sales")]
//         public async Task<IActionResult> GetSalesReport(
//             [FromQuery] DateTime? from,
//             [FromQuery] DateTime? to,
//             [FromQuery] string? period = "month") // day, week, month, year
//         {
//             var (startDate, endDate) = GetDateRange(period, from, to);

//             var sales = await _context.Invoices
//                 .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
//                 .Select(i => new
//                 {
//                     i.InvoiceNumber,
//                     i.InvoiceDate,
//                     i.TotalAmount,
//                     i.Status,
//                     CustomerName = i.Customer.CustomerName,
//                     BranchName = i.Branch.BranchName,
//                     i.InvoiceItems
//                 })
//                 .ToListAsync();

//             var totalSales = sales.Sum(i => i.TotalAmount);
//             var totalInvoices = sales.Count;

//             return Ok(new
//             {
//                 period,
//                 startDate,
//                 endDate,
//                 totalSales,
//                 totalInvoices,
//                 data = sales
//             });
//         }

//         // ==================== 📊 تقرير الأرباح ====================

//         // GET: api/reports/profit
//         [HttpGet("profit")]
//         public async Task<IActionResult> GetProfitReport(
//             [FromQuery] DateTime? from,
//             [FromQuery] DateTime? to,
//             [FromQuery] string? period = "month")
//         {
//             var (startDate, endDate) = GetDateRange(period, from, to);

//             // إجمالي المبيعات
//             var totalSales = await _context.Invoices
//                 .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
//                 .SumAsync(i => i.TotalAmount);

//             // إجمالي التكاليف (من حركات الشراء)
//             var totalCost = await _context.StockMovements
//                 .Where(m => m.MovementType == "إدخال" && m.CreatedAt >= startDate && m.CreatedAt <= endDate)
//                 .SumAsync(m => m.TotalPrice);

//             // إجمالي الضرائب
//             var totalTax = await _context.Invoices
//                 .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
//                 .SumAsync(i => i.TaxAmount);

//             // إجمالي الخصومات
//             var totalDiscount = await _context.Invoices
//                 .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
//                 .SumAsync(i => i.DiscountAmount);

//             var netProfit = totalSales - totalCost;

//             return Ok(new
//             {
//                 period,
//                 startDate,
//                 endDate,
//                 totalSales,
//                 totalCost,
//                 totalTax,
//                 totalDiscount,
//                 netProfit,
//                 profitMargin = totalSales > 0 ? (netProfit / totalSales) * 100 : 0
//             });
//         }

//         // ==================== 📊 تقرير المخزون ====================

//         // GET: api/reports/inventory
//         [HttpGet("inventory")]
//         public async Task<IActionResult> GetInventoryReport()
//         {
//             var inventory = await _context.ProductWarehouses
//                 .Include(pw => pw.Product)
//                 .Include(pw => pw.Warehouse)
//                 .Where(pw => pw.Quantity > 0)
//                 .Select(pw => new
//                 {
//                     pw.ProductId,
//                     ProductName = pw.Product.ProductName,
//                     ProductCode = pw.Product.ProductCode,
//                     pw.WarehouseId,
//                     WarehouseName = pw.Warehouse.WarehouseName,
//                     pw.Quantity,
//                     pw.ReorderPoint,
//                     TotalValue = pw.Quantity * pw.Product.PurchasePrice,
//                     pw.Product.PurchasePrice,
//                     pw.Product.SellingPrice,
//                     Status = pw.Quantity < pw.ReorderPoint ? "منخفض" : "متوفر"
//                 })
//                 .ToListAsync();

//             var totalItems = inventory.Count;
//             var totalValue = inventory.Sum(i => i.TotalValue);
//             var lowStockItems = inventory.Where(i => i.Status == "منخفض").ToList();

//             return Ok(new
//             {
//                 totalItems,
//                 totalValue,
//                 lowStockCount = lowStockItems.Count,
//                 lowStockItems,
//                 inventory
//             });
//         }

//         // ==================== 📊 تقرير أداء الموظفين ====================

//         // GET: api/reports/employees
//         [HttpGet("employees")]
//         public async Task<IActionResult> GetEmployeePerformanceReport(
//             [FromQuery] DateTime? from,
//             [FromQuery] DateTime? to,
//             [FromQuery] string? period = "month")
//         {
//             var (startDate, endDate) = GetDateRange(period, from, to);

//             var employees = await _context.Users
//                 .Where(u => u.IsActive)
//                 .Select(u => new
//                 {
//                     u.Id,
//                     u.FullName,
//                     u.Username,
//                     u.Role.RoleName,
//                     InvoicesCount = _context.Invoices
//                         .Count(i => i.UserId == u.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة"),
//                     TotalSales = _context.Invoices
//                         .Where(i => i.UserId == u.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
//                         .Sum(i => i.TotalAmount),
//                     TotalItems = _context.InvoiceItems
//                         .Where(ii => ii.Invoice.UserId == u.Id && ii.Invoice.InvoiceDate >= startDate && ii.Invoice.InvoiceDate <= endDate && ii.Invoice.Status != "ملغاة")
//                         .Sum(ii => ii.Quantity)
//                 })
//                 .OrderByDescending(u => u.TotalSales)
//                 .ToListAsync();

//             return Ok(new
//             {
//                 period,
//                 startDate,
//                 endDate,
//                 employees,
//                 totalEmployees = employees.Count
//             });
//         }

//         // ==================== 📊 تقرير العملاء ====================

//         // GET: api/reports/customers
//         [HttpGet("customers")]
//         public async Task<IActionResult> GetCustomerReport(
//             [FromQuery] DateTime? from,
//             [FromQuery] DateTime? to,
//             [FromQuery] string? period = "month")
//         {
//             var (startDate, endDate) = GetDateRange(period, from, to);

//             var customers = await _context.Customers
//                 .Where(c => c.IsActive)
//                 .Select(c => new
//                 {
//                     c.Id,
//                     c.CustomerName,
//                     c.Phone,
//                     c.Email,
//                     TotalPurchases = _context.Invoices
//                         .Where(i => i.CustomerId == c.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
//                         .Sum(i => i.TotalAmount),
//                     OrdersCount = _context.Invoices
//                         .Count(i => i.CustomerId == c.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة"),
//                     LastPurchase = _context.Invoices
//                         .Where(i => i.CustomerId == c.Id && i.Status != "ملغاة")
//                         .OrderByDescending(i => i.InvoiceDate)
//                         .Select(i => i.InvoiceDate)
//                         .FirstOrDefault()
//                 })
//                 .Where(c => c.TotalPurchases > 0)
//                 .OrderByDescending(c => c.TotalPurchases)
//                 .ToListAsync();

//             return Ok(new
//             {
//                 period,
//                 startDate,
//                 endDate,
//                 topCustomers = customers.Take(10),
//                 totalCustomers = customers.Count,
//                 totalRevenue = customers.Sum(c => c.TotalPurchases)
//             });
//         }

//         // ==================== 📊 تقرير مبيعات الفرع ====================

//         // GET: api/reports/branches
//         [HttpGet("branches")]
//         public async Task<IActionResult> GetBranchReport(
//             [FromQuery] DateTime? from,
//             [FromQuery] DateTime? to,
//             [FromQuery] string? period = "month")
//         {
//             var (startDate, endDate) = GetDateRange(period, from, to);

//             var branches = await _context.Branches
//                 .Where(b => b.IsActive)
//                 .Select(b => new
//                 {
//                     b.Id,
//                     b.BranchName,
//                     b.Address,
//                     TotalSales = _context.Invoices
//                         .Where(i => i.BranchId == b.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
//                         .Sum(i => i.TotalAmount),
//                     OrdersCount = _context.Invoices
//                         .Count(i => i.BranchId == b.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة"),
//                     UniqueCustomers = _context.Invoices
//                         .Where(i => i.BranchId == b.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
//                         .Select(i => i.CustomerId)
//                         .Distinct()
//                         .Count()
//                 })
//                 .OrderByDescending(b => b.TotalSales)
//                 .ToListAsync();

//             return Ok(new
//             {
//                 period,
//                 startDate,
//                 endDate,
//                 branches,
//                 totalSales = branches.Sum(b => b.TotalSales),
//                 totalOrders = branches.Sum(b => b.OrdersCount)
//             });
//         }

//         // ==================== 🛠️ دوال مساعدة ====================

//         private (DateTime startDate, DateTime endDate) GetDateRange(string? period, DateTime? from, DateTime? to)
//         {
//             var now = DateTime.Now;

//             if (from.HasValue && to.HasValue)
//                 return (from.Value, to.Value);

//             return period?.ToLower() switch
//             {
//                 "today" => (now.Date, now.Date.AddDays(1).AddSeconds(-1)),
//                 "week" => (now.AddDays(-7), now),
//                 "month" => (new DateTime(now.Year, now.Month, 1), now),
//                 "year" => (new DateTime(now.Year, 1, 1), now),
//                 _ => (new DateTime(now.Year, now.Month, 1), now)
//             };
//         }
//     }
// }






using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Data;

namespace POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== 📊 تقرير المبيعات ====================

        // GET: api/reports/sales
        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? period = "month") // day, week, month, year
        {
            var (startDate, endDate) = GetDateRange(period, from, to);

            var sales = await _context.Invoices
                .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
                .Select(i => new
                {
                    i.InvoiceNumber,
                    i.InvoiceDate,
                    i.TotalAmount,
                    i.Status,
                    CustomerName = i.Customer.CustomerName,
                    BranchName = i.Branch.BranchName,
                    i.InvoiceItems
                })
                .ToListAsync();

            var totalSales = sales.Sum(i => i.TotalAmount);
            var totalInvoices = sales.Count;

            return Ok(new
            {
                period,
                startDate,
                endDate,
                totalSales,
                totalInvoices,
                data = sales
            });
        }

        // ==================== 📊 تقرير الأرباح ====================

        // GET: api/reports/profit
        [HttpGet("profit")]
        public async Task<IActionResult> GetProfitReport(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? period = "month")
        {
            var (startDate, endDate) = GetDateRange(period, from, to);

            // إجمالي المبيعات
            var totalSales = await _context.Invoices
                .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
                .SumAsync(i => i.TotalAmount);

            // إجمالي التكاليف (من حركات الشراء)
            var totalCost = await _context.StockMovements
                .Where(m => m.MovementType == "إدخال" && m.CreatedAt >= startDate && m.CreatedAt <= endDate)
                .SumAsync(m => m.TotalPrice);

            // إجمالي الضرائب
            var totalTax = await _context.Invoices
                .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
                .SumAsync(i => i.TaxAmount);

            // إجمالي الخصومات
            var totalDiscount = await _context.Invoices
                .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
                .SumAsync(i => i.DiscountAmount);

            var netProfit = totalSales - totalCost;

            return Ok(new
            {
                period,
                startDate,
                endDate,
                totalSales,
                totalCost,
                totalTax,
                totalDiscount,
                netProfit,
                profitMargin = totalSales > 0 ? (netProfit / totalSales) * 100 : 0
            });
        }

        // ==================== 📊 تقرير المخزون ====================

        // GET: api/reports/inventory
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventoryReport()
        {
            var inventory = await _context.ProductWarehouses
                .Include(pw => pw.Product)
                .Include(pw => pw.Warehouse)
                .Where(pw => pw.Quantity > 0)
                .Select(pw => new
                {
                    pw.ProductId,
                    ProductName = pw.Product.ProductName,
                    ProductCode = pw.Product.ProductCode,
                    pw.WarehouseId,
                    WarehouseName = pw.Warehouse.WarehouseName,
                    pw.Quantity,
                    pw.ReorderPoint,
                    TotalValue = pw.Quantity * pw.Product.PurchasePrice,
                    pw.Product.PurchasePrice,
                    pw.Product.SellingPrice,
                    Status = pw.Quantity < pw.ReorderPoint ? "منخفض" : "متوفر"
                })
                .ToListAsync();

            var totalItems = inventory.Count;
            var totalValue = inventory.Sum(i => i.TotalValue);
            var lowStockItems = inventory.Where(i => i.Status == "منخفض").ToList();

            return Ok(new
            {
                totalItems,
                totalValue,
                lowStockCount = lowStockItems.Count,
                lowStockItems,
                inventory
            });
        }

        // ==================== 📊 تقرير أداء الموظفين ====================

        // GET: api/reports/employees
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployeePerformanceReport(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? period = "month")
        {
            var (startDate, endDate) = GetDateRange(period, from, to);

            var employees = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Username,
                    u.Role.RoleName,
                    InvoicesCount = _context.Invoices
                        .Count(i => i.UserId == u.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة"),
                    TotalSales = _context.Invoices
                        .Where(i => i.UserId == u.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
                        .Sum(i => i.TotalAmount),
                    TotalItems = _context.InvoiceItems
                        .Where(ii => ii.Invoice.UserId == u.Id && ii.Invoice.InvoiceDate >= startDate && ii.Invoice.InvoiceDate <= endDate && ii.Invoice.Status != "ملغاة")
                        .Sum(ii => ii.Quantity)
                })
                .OrderByDescending(u => u.TotalSales)
                .ToListAsync();

            return Ok(new
            {
                period,
                startDate,
                endDate,
                employees,
                totalEmployees = employees.Count
            });
        }

        // ==================== 📊 تقرير العملاء ====================

        // GET: api/reports/customers
        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomerReport(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? period = "month")
        {
            var (startDate, endDate) = GetDateRange(period, from, to);

            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.Id,
                    c.CustomerName,
                    c.Phone,
                    c.Email,
                    TotalPurchases = _context.Invoices
                        .Where(i => i.CustomerId == c.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
                        .Sum(i => i.TotalAmount),
                    OrdersCount = _context.Invoices
                        .Count(i => i.CustomerId == c.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة"),
                    LastPurchase = _context.Invoices
                        .Where(i => i.CustomerId == c.Id && i.Status != "ملغاة")
                        .OrderByDescending(i => i.InvoiceDate)
                        .Select(i => i.InvoiceDate)
                        .FirstOrDefault()
                })
                .Where(c => c.TotalPurchases > 0)
                .OrderByDescending(c => c.TotalPurchases)
                .ToListAsync();

            return Ok(new
            {
                period,
                startDate,
                endDate,
                topCustomers = customers.Take(10),
                totalCustomers = customers.Count,
                totalRevenue = customers.Sum(c => c.TotalPurchases)
            });
        }

        // ==================== 📊 تقرير مبيعات الفرع ====================

        // GET: api/reports/branches
        [HttpGet("branches")]
        public async Task<IActionResult> GetBranchReport(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? period = "month")
        {
            var (startDate, endDate) = GetDateRange(period, from, to);

            var branches = await _context.Branches
                .Where(b => b.IsActive)
                .Select(b => new
                {
                    b.Id,
                    b.BranchName,
                    b.Address,
                    TotalSales = _context.Invoices
                        .Where(i => i.BranchId == b.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
                        .Sum(i => i.TotalAmount),
                    OrdersCount = _context.Invoices
                        .Count(i => i.BranchId == b.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة"),
                    UniqueCustomers = _context.Invoices
                        .Where(i => i.BranchId == b.Id && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status != "ملغاة")
                        .Select(i => i.CustomerId)
                        .Distinct()
                        .Count()
                })
                .OrderByDescending(b => b.TotalSales)
                .ToListAsync();

            return Ok(new
            {
                period,
                startDate,
                endDate,
                branches,
                totalSales = branches.Sum(b => b.TotalSales),
                totalOrders = branches.Sum(b => b.OrdersCount)
            });
        }

        // ==================== 📊 تقرير المنتجات الأكثر مبيعاً (الكود المضاف) ====================

        // GET: api/reports/top-products
        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProductsReport(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? period = "month",
            [FromQuery] int top = 5) // القيمة الافتراضية 5 كما طلبت الواجهة
        {
            var (startDate, endDate) = GetDateRange(period, from, to);

            var topProducts = await _context.InvoiceItems
                .Include(ii => ii.Invoice)
                .Include(ii => ii.Product)
                .Where(ii => ii.Invoice.InvoiceDate >= startDate && 
                             ii.Invoice.InvoiceDate <= endDate && 
                             ii.Invoice.Status != "ملغاة")
                .GroupBy(ii => new { ii.ProductId, ii.Product.ProductName })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalQuantitySold = g.Sum(ii => ii.Quantity),
                    TotalSalesValue = g.Sum(ii => ii.TotalPrice)
                })
                .OrderByDescending(p => p.TotalQuantitySold)
                .Take(top)
                .ToListAsync();

            return Ok(new
            {
                period,
                startDate,
                endDate,
                top,
                data = topProducts
            });
        }

        // ==================== 🛠️ دوال مساعدة ====================

        private (DateTime startDate, DateTime endDate) GetDateRange(string? period, DateTime? from, DateTime? to)
        {
            var now = DateTime.Now;

            if (from.HasValue && to.HasValue)
                return (from.Value, to.Value);

            return period?.ToLower() switch
            {
                "today" => (now.Date, now.Date.AddDays(1).AddSeconds(-1)),
                "week" => (now.AddDays(-7), now),
                "month" => (new DateTime(now.Year, now.Month, 1), now),
                "year" => (new DateTime(now.Year, 1, 1), now),
                _ => (new DateTime(now.Year, now.Month, 1), now)
            };
        }
    }
}