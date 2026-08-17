using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;

namespace POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/payments
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? invoiceId,
            [FromQuery] int? methodId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var query = _context.Payments
                .Include(p => p.Invoice)
                .Include(p => p.PaymentMethod)
                .AsQueryable();

            if (invoiceId.HasValue)
                query = query.Where(p => p.InvoiceId == invoiceId.Value);

            if (methodId.HasValue)
                query = query.Where(p => p.PaymentMethodId == methodId.Value);

            if (from.HasValue)
                query = query.Where(p => p.PaymentDate >= from.Value);

            if (to.HasValue)
                query = query.Where(p => p.PaymentDate <= to.Value);

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new
                {
                    p.Id,
                    p.Amount,
                    p.PaymentDate,
                    p.ReferenceNumber,
                    p.Notes,
                    InvoiceNumber = p.Invoice.InvoiceNumber,
                    PaymentMethod = p.PaymentMethod.MethodName
                })
                .ToListAsync();

            return Ok(payments);
        }

        // GET: api/payments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Invoice)
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound(new { message = "الدفعة غير موجودة" });

            return Ok(payment);
        }

        // POST: api/payments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
        {
            var invoice = await _context.Invoices.FindAsync(request.InvoiceId);
            if (invoice == null)
                return NotFound(new { message = "الفاتورة غير موجودة" });

            var method = await _context.PaymentMethods.FindAsync(request.PaymentMethodId);
            if (method == null)
                return NotFound(new { message = "طريقة الدفع غير موجودة" });

            var payment = new Payment
            {
                InvoiceId = request.InvoiceId,
                PaymentMethodId = request.PaymentMethodId,
                Amount = request.Amount,
                ReferenceNumber = request.ReferenceNumber,
                PaymentDate = DateTime.Now,
                Notes = request.Notes,
                CreatedAt = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // تحديث حالة الفاتورة إذا تم دفعها بالكامل
            var totalPaid = await _context.Payments
                .Where(p => p.InvoiceId == request.InvoiceId)
                .SumAsync(p => p.Amount);

            if (totalPaid >= invoice.TotalAmount)
            {
                invoice.Status = "مدفوعة";
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }

        // DELETE: api/payments/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "مدير النظام")]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound(new { message = "الدفعة غير موجودة" });

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف الدفعة بنجاح" });
        }
    }

    public class CreatePaymentRequest
    {
        public int InvoiceId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
}