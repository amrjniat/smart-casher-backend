using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;
using System.Security.Claims;
using System.Data;

namespace POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvoicesController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== 📋 CRUD ====================

        // GET: api/invoices
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] int? customerId,
            [FromQuery] int? branchId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Branch)
                .Include(i => i.User)
                .Include(i => i.InvoiceItems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(i => i.Status == status);

            if (customerId.HasValue)
                query = query.Where(i => i.CustomerId == customerId.Value);

            if (branchId.HasValue)
                query = query.Where(i => i.BranchId == branchId.Value);

            if (from.HasValue)
                query = query.Where(i => i.InvoiceDate >= from.Value);

            if (to.HasValue)
                query = query.Where(i => i.InvoiceDate <= to.Value);

            var total = await query.CountAsync();

            var invoices = await query
                .OrderByDescending(i => i.InvoiceDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceNumber,
                    i.InvoiceDate,
                    i.SubTotal,
                    i.TaxAmount,
                    i.DiscountAmount,
                    i.TotalAmount,
                    i.Status,
                    i.Notes,
                    CustomerName = i.Customer.CustomerName,
                    BranchName = i.Branch.BranchName,
                    UserName = i.User != null ? i.User.FullName : null,
                    ItemsCount = i.InvoiceItems.Count
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, invoices });
        }

        // GET: api/invoices/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Branch)
                .Include(i => i.User)
                .Include(i => i.InvoiceItems)
                .ThenInclude(ii => ii.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound(new { message = "الفاتورة غير موجودة" });

            return Ok(invoice);
        }

        // POST: api/invoices
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request)
        {
            if (request.Items.Count == 0 || request.Items.Any(item => item.Quantity <= 0))
                return BadRequest(new { message = "يجب أن تحتوي الفاتورة على عناصر بكميات موجبة" });

            if (request.Items.Any(item => item.UnitPrice.HasValue && item.UnitPrice.Value < 0))
                return BadRequest(new { message = "سعر البيع لا يمكن أن يكون سالباً" });

            if (request.DiscountAmount < 0)
                return BadRequest(new { message = "الخصم لا يمكن أن يكون سالباً" });

            // التحقق من العميل
            var customer = await _context.Customers.FindAsync(request.CustomerId);
            if (customer == null)
                return NotFound(new { message = "العميل غير موجود" });

            // التحقق من الفرع
            var branch = await _context.Branches.FindAsync(request.BranchId);
            if (branch == null)
                return NotFound(new { message = "الفرع غير موجود" });

            var currentUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == GetCurrentUserId());
            if (currentUser == null)
                return Unauthorized(new { message = "المستخدم غير موجود" });

            var isAdministrator = currentUser.Role?.RoleName is "مدير النظام" or "Admin" or "Administrator";
            if (!isAdministrator && (!currentUser.BranchId.HasValue || currentUser.BranchId.Value != request.BranchId))
                return Forbid();

            var assignedWarehouseId = !isAdministrator
                ? await _context.Warehouses
                    .Where(w => w.IsActive && w.BranchId == currentUser.BranchId)
                    .OrderByDescending(w => w.IsMainWarehouse)
                    .ThenBy(w => w.Id)
                    .Select(w => (int?)w.Id)
                    .FirstOrDefaultAsync()
                : null;
            if (!isAdministrator && (!assignedWarehouseId.HasValue || assignedWarehouseId.Value != request.WarehouseId))
                return Forbid();

            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == request.WarehouseId && w.IsActive);
            if (warehouse == null || warehouse.BranchId != request.BranchId)
                return BadRequest(new { message = "المستودع غير متاح لهذا الفرع" });

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            // التحقق من المنتجات والتأكد من توفر الكمية
            foreach (var item in request.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    return BadRequest(new { message = $"المنتج {item.ProductId} غير موجود" });

                var stock = await _context.ProductWarehouses
                    .Where(pw => pw.ProductId == item.ProductId && pw.WarehouseId == request.WarehouseId)
                    .SumAsync(pw => pw.Quantity);

                if (stock < item.Quantity)
                    return BadRequest(new { message = $"الكمية غير متوفرة للمنتج {product.ProductName}" });
            }

            // إنشاء الفاتورة
            var invoiceNumber = GenerateInvoiceNumber();

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                CustomerId = request.CustomerId,
                BranchId = request.BranchId,
                UserId = GetCurrentUserId(),
                InvoiceDate = DateTime.Now,
                SubTotal = 0,
                TaxAmount = 0,
                DiscountAmount = request.DiscountAmount ?? 0,
                TotalAmount = 0,
                Status = "مدفوعة",
                Notes = request.Notes,
                CreatedAt = DateTime.Now
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // إضافة تفاصيل الفاتورة وتحديث المخزون
            decimal subTotal = 0;
            decimal taxAmount = 0;

            foreach (var item in request.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                var unitPrice = product!.SellingPrice;
                var totalPrice = unitPrice * item.Quantity;
                var taxRate = product!.TaxRate;
                var itemTax = totalPrice * (taxRate / 100);

                var invoiceItem = new InvoiceItem
                {
                    InvoiceId = invoice.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    TaxRate = taxRate,
                    TaxAmount = itemTax
                };

                _context.InvoiceItems.Add(invoiceItem);

                subTotal += totalPrice;
                taxAmount += itemTax;

                // خصم الكمية من المخزون
                if (!await UpdateStock(item.ProductId, request.WarehouseId, item.Quantity))
                    return BadRequest(new { message = $"الكمية غير متوفرة للمنتج {product.ProductName}" });
            }

            var discountAmount = request.DiscountAmount ?? 0;
            if (discountAmount > subTotal)
                return BadRequest(new { message = "الخصم لا يمكن أن يتجاوز إجمالي المنتجات" });

            // تحديث إجمالي الفاتورة
            invoice.SubTotal = subTotal;
            invoice.TaxAmount = taxAmount;
            invoice.DiscountAmount = discountAmount;
            invoice.TotalAmount = subTotal + taxAmount - discountAmount;

            if (request.PaidAmount.HasValue)
            {
                if (request.PaidAmount.Value <= 0)
                    return BadRequest(new { message = "مبلغ الدفع يجب أن يكون أكبر من صفر" });

                if (!request.PaymentMethodId.HasValue)
                    return BadRequest(new { message = "يجب تحديد طريقة الدفع" });

                if (request.PaidAmount.Value > invoice.TotalAmount)
                    return BadRequest(new { message = "مبلغ الدفع يتجاوز إجمالي الفاتورة" });

                var paymentMethod = await _context.PaymentMethods.FindAsync(request.PaymentMethodId.Value);
                if (paymentMethod == null)
                    return NotFound(new { message = "طريقة الدفع غير موجودة" });

                _context.Payments.Add(new Payment
                {
                    InvoiceId = invoice.Id,
                    PaymentMethodId = paymentMethod.Id,
                    Amount = request.PaidAmount.Value,
                    PaymentDate = DateTime.Now,
                    Notes = "دفعة عند إنشاء الفاتورة",
                    CreatedAt = DateTime.Now
                });

                if (request.PaidAmount.Value >= invoice.TotalAmount)
                    invoice.Status = "مدفوعة";
            }

            // تسجيل حركة إخراج
            foreach (var item in request.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                var movement = new StockMovement
                {
                    MovementNumber = GenerateMovementNumber("INV"),
                    ProductId = item.ProductId,
                    WarehouseId = request.WarehouseId,
                    MovementType = "إخراج",
                    Quantity = item.Quantity,
                    UnitPrice = product!.SellingPrice,
                    TotalPrice = product.SellingPrice * item.Quantity,
                    Notes = $"فاتورة #{invoiceNumber}",
                    CustomerId = request.CustomerId,
                    UserId = GetCurrentUserId(),
                    CreatedAt = DateTime.Now
                };
                _context.StockMovements.Add(movement);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
        }

        // PUT: api/invoices/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (request.Status is not ("مدفوعة" or "غير مدفوعة" or "ملغاة"))
                return BadRequest(new { message = "حالة الفاتورة غير صالحة" });

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return NotFound(new { message = "الفاتورة غير موجودة" });

            invoice.Status = request.Status;
            invoice.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تحديث حالة الفاتورة", invoice });
        }

        // DELETE: api/invoices/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,مدير النظام,Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound(new { message = "الفاتورة غير موجودة" });

            // حذف ناعم
            invoice.IsActive = false;
            invoice.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف الفاتورة بنجاح" });
        }

        // ==================== 🛠️ دوال مساعدة ====================

        private string GenerateInvoiceNumber()
        {
            var date = DateTime.Now.ToString("yyMMdd");
            var count = _context.Invoices.Count() + 1;
            return $"INV-{date}-{count:D4}";
        }

        private string GenerateMovementNumber(string prefix)
        {
            var date = DateTime.Now.ToString("yyMMdd");
            var count = _context.StockMovements.Count() + 1;
            return $"{prefix}-{date}-{count:D4}";
        }

        private int GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId != null ? int.Parse(userId) : 0;
        }

        private async Task<bool> UpdateStock(int productId, int warehouseId, int quantity)
        {
            var productWarehouse = await _context.ProductWarehouses
            .Where(pw => pw.ProductId == productId && pw.WarehouseId == warehouseId && pw.Quantity >= quantity)
                .FirstOrDefaultAsync();

            if (productWarehouse == null)
                return false;

            productWarehouse.Quantity -= quantity;
            return true;
        }
    }

    // 📦 DTOs

    public class CreateInvoiceRequest
    {
        public int CustomerId { get; set; }
        public int BranchId { get; set; }
        public int WarehouseId { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public int? PaymentMethodId { get; set; }
        public string? Notes { get; set; }
        public List<InvoiceItemRequest> Items { get; set; } = new();
    }

    public class InvoiceItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty; // "مدفوعة", "غير مدفوعة", "ملغاة"
    }
}