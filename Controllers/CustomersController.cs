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
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== 📊 1. إحصائيات العملاء ====================
        // GET: api/customers/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] string period = "today")
        {
            var now = DateTime.Now;
            var startDate = period.ToLower() switch
            {
                "today" => now.Date,
                "week" => now.AddDays(-7),
                "month" => new DateTime(now.Year, now.Month, 1),
                "year" => new DateTime(now.Year, 1, 1),
                _ => now.Date
            };

            var totalSales = await _context.Invoices
                .Where(i => i.InvoiceDate >= startDate && i.Status != "ملغاة")
                .SumAsync(i => i.TotalAmount);

            var activeCustomers = await _context.Customers.CountAsync(c => c.IsActive);
            var newCustomers = await _context.Customers.CountAsync(c => c.CreatedAt >= startDate && c.IsActive);
            var totalCustomers = await _context.Customers.CountAsync();
            var customersWithDebt = await _context.Customers.CountAsync(c => c.CurrentBalance < 0 && c.IsActive);

            return Ok(new
            {
                period,
                startDate,
                totalSales,
                activeCustomers,
                newCustomers,
                totalCustomers,
                customersWithDebt
            });
        }

        // ==================== 📊 2. ملخص سريع ====================
        // GET: api/customers/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.Now.Date;

            var totalSales = await _context.Invoices
                .Where(i => i.Status != "ملغاة")
                .SumAsync(i => i.TotalAmount);

            var totalCustomers = await _context.Customers.CountAsync(c => c.IsActive);
            var newCustomersToday = await _context.Customers
                .CountAsync(c => c.CreatedAt.Date == today && c.IsActive);
            var activeCustomers = await _context.Customers
                .CountAsync(c => c.IsActive);

            return Ok(new
            {
                totalSales,
                totalCustomers,
                newCustomersToday,
                activeCustomers
            });
        }

        // ==================== 📋 3. جلب قائمة العملاء ====================
        // GET: api/customers
        [HttpGet]
        public async Task<IActionResult> GetCustomers(
            [FromQuery] string? search = null,
            [FromQuery] string? phone = null,
            [FromQuery] bool? hasDebt = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Customers.Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(c =>
                    c.CustomerName.ToLower().Contains(searchLower) ||
                    (c.CustomerCode != null && c.CustomerCode.ToLower().Contains(searchLower))
                );
            }

            if (!string.IsNullOrEmpty(phone))
            {
                var phoneLower = phone.ToLower();
                query = query.Where(c =>
                    (c.Phone != null && c.Phone.ToLower().Contains(phoneLower)) ||
                    (c.Mobile != null && c.Mobile.ToLower().Contains(phoneLower))
                );
            }

            if (hasDebt.HasValue)
            {
                query = hasDebt.Value
                    ? query.Where(c => c.CurrentBalance < 0)
                    : query.Where(c => c.CurrentBalance >= 0);
            }

            var totalItems = await query.CountAsync();

            var customers = await query
                .OrderByDescending(c => c.TotalPurchases)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.CustomerName,
                    c.CustomerCode,
                    c.Phone,
                    c.Mobile,
                    c.Email,
                    c.Address,
                    c.OpeningBalance,
                    c.CurrentBalance,
                    c.TotalPurchases,
                    c.LoyaltyPoints,
                    c.Notes,
                    c.IsActive,
                    c.CreatedAt,
                    InvoicesCount = _context.Invoices.Count(i => i.CustomerId == c.Id && i.Status != "ملغاة"),
                    Status = c.CurrentBalance < 0 ? "مدين" :
                             c.CreatedAt >= DateTime.Now.AddDays(-7) ? "جديد" :
                             c.IsActive ? "نشط" : "غير نشط"
                })
                .ToListAsync();

            return Ok(new
            {
                totalItems,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                customers
            });
        }

        // ==================== 👤 4. جلب عميل معين ====================
        // GET: api/customers/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .Select(c => new
                {
                    c.Id,
                    c.CustomerName,
                    c.CustomerCode,
                    c.Phone,
                    c.Mobile,
                    c.Email,
                    c.Address,
                    c.OpeningBalance,
                    c.CurrentBalance,
                    c.TotalPurchases,
                    c.LoyaltyPoints,
                    c.Notes,
                    c.IsActive,
                    c.CreatedAt,
                    c.UpdatedAt,
                    InvoicesCount = _context.Invoices.Count(i => i.CustomerId == c.Id && i.Status != "ملغاة")
                })
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound(new { message = "العميل غير موجود" });

            return Ok(customer);
        }

        // ==================== ➕ 5. إضافة عميل جديد ====================
        // POST: api/customers
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
        {
            if (!string.IsNullOrEmpty(request.Phone))
            {
                var existing = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Phone == request.Phone && c.IsActive);
                if (existing != null)
                    return BadRequest(new { message = "يوجد عميل بنفس رقم الهاتف" });
            }

            var customer = new Customer
            {
                CustomerName = request.CustomerName,
                CustomerCode = request.CustomerCode ?? GenerateCustomerCode(),
                Phone = request.Phone,
                Mobile = request.Mobile,
                Email = request.Email,
                Address = request.Address,
                TaxNumber = request.TaxNumber,
                OpeningBalance = request.OpeningBalance ?? 0,
                CurrentBalance = request.OpeningBalance ?? 0,
                TotalPurchases = 0,
                LoyaltyPoints = request.LoyaltyPoints ?? 0,
                Notes = request.Notes,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }

        // ==================== ✏️ 6. تحديث عميل ====================
        // PUT: api/customers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest request)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound(new { message = "العميل غير موجود" });

            if (!string.IsNullOrEmpty(request.Phone) && request.Phone != customer.Phone)
            {
                var existing = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Phone == request.Phone && c.Id != id && c.IsActive);
                if (existing != null)
                    return BadRequest(new { message = "رقم الهاتف مستخدم من قبل عميل آخر" });
            }

            customer.CustomerName = request.CustomerName ?? customer.CustomerName;
            customer.CustomerCode = request.CustomerCode ?? customer.CustomerCode;
            customer.Phone = request.Phone ?? customer.Phone;
            customer.Mobile = request.Mobile ?? customer.Mobile;
            customer.Email = request.Email ?? customer.Email;
            customer.Address = request.Address ?? customer.Address;
            customer.TaxNumber = request.TaxNumber ?? customer.TaxNumber;
            customer.Notes = request.Notes ?? customer.Notes;
            customer.LoyaltyPoints = request.LoyaltyPoints ?? customer.LoyaltyPoints;
            customer.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تحديث العميل", customer });
        }

        // ==================== 🗑️ 7. حذف عميل ====================
        // DELETE: api/customers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound(new { message = "العميل غير موجود" });

            var hasInvoices = await _context.Invoices
                .AnyAsync(i => i.CustomerId == id && i.Status != "ملغاة");

            if (hasInvoices)
                return BadRequest(new { message = "لا يمكن حذف العميل لأنه لديه فواتير" });

            customer.IsActive = false;
            customer.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف العميل" });
        }

        // ==================== 📄 8. سجل مشتريات العميل ====================
        // GET: api/customers/{id}/invoices
        [HttpGet("{id}/invoices")]
        public async Task<IActionResult> GetCustomerInvoices(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound(new { message = "العميل غير موجود" });

            var query = _context.Invoices
                .Include(i => i.Branch)
                .Where(i => i.CustomerId == id && i.Status != "ملغاة")
                .OrderByDescending(i => i.InvoiceDate);

            var totalItems = await query.CountAsync();

            var invoices = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceNumber,
                    i.InvoiceDate,
                    i.TotalAmount,
                    i.Status,
                    BranchName = i.Branch.BranchName,
                    ItemsCount = i.InvoiceItems.Count
                })
                .ToListAsync();

            return Ok(new
            {
                customer = new { customer.Id, customer.CustomerName },
                totalItems,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                invoices
            });
        }

        // ==================== 🔍 9. بحث سريع ====================
        // GET: api/customers/search?query=أحمد
        [HttpGet("search")]
        public async Task<IActionResult> SearchCustomers([FromQuery] string query, [FromQuery] int take = 10)
        {
            if (string.IsNullOrEmpty(query) || query.Length < 2)
                return Ok(new List<object>());

            var queryLower = query.ToLower();

            var customers = await _context.Customers
                .Where(c => c.IsActive &&
                    (c.CustomerName.ToLower().Contains(queryLower) ||
                     c.Phone.ToLower().Contains(queryLower) ||
                     (c.Mobile != null && c.Mobile.ToLower().Contains(queryLower))))
                .OrderBy(c => c.CustomerName)
                .Take(take)
                .Select(c => new
                {
                    c.Id,
                    c.CustomerName,
                    c.Phone,
                    c.Mobile,
                    c.CurrentBalance
                })
                .ToListAsync();

            return Ok(customers);
        }

        // ==================== 🛠️ دوال مساعدة ====================

        private string GenerateCustomerCode()
        {
            var count = _context.Customers.Count() + 1;
            return $"CUS-{DateTime.Now:yyMMdd}-{count:D4}";
        }
    }


}




