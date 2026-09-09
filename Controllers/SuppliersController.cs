using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.DTOs;
using POS.Models;

namespace POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SuppliersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SuppliersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSuppliers()
        {
            var suppliers = await _context.Suppliers
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    s.Id,
                    s.SupplierName,
                    s.SupplierCode,
                    s.ContactPerson,
                    s.Phone,
                    s.Mobile,
                    s.Email,
                    s.Address,
                    s.TaxNumber,
                    s.CommercialRegister,
                    s.OpeningBalance,
                    s.CurrentBalance,
                    s.Notes,
                    s.IsActive,
                    Status = s.IsActive ? "active" : "inactive",
                    StatusAr = s.IsActive ? "نشط" : "موقوف",
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            return Ok(suppliers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupplier(int id)
        {
            var supplier = await _context.Suppliers
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.SupplierName,
                    s.SupplierCode,
                    s.ContactPerson,
                    s.Phone,
                    s.Mobile,
                    s.Email,
                    s.Address,
                    s.TaxNumber,
                    s.CommercialRegister,
                    s.OpeningBalance,
                    s.CurrentBalance,
                    s.Notes,
                    s.IsActive,
                    Status = s.IsActive ? "active" : "inactive",
                    StatusAr = s.IsActive ? "نشط" : "موقوف",
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (supplier == null)
                return NotFound(new { message = "المورد غير موجود" });

            return Ok(supplier);
        }

        [HttpPost]
        public async Task<IActionResult> PostSupplier([FromBody] CreateSupplierRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SupplierName))
                return BadRequest(new { message = "اسم المورد مطلوب" });

            var isActive = ResolveIsActive(request.IsActive, request.Status, true);

            var supplier = new Supplier
            {
                SupplierName = request.SupplierName.Trim(),
                SupplierCode = request.SupplierCode,
                ContactPerson = request.ContactPerson,
                Phone = request.Phone,
                Mobile = request.Mobile,
                Email = request.Email,
                Address = request.Address,
                TaxNumber = request.TaxNumber,
                CommercialRegister = request.CommercialRegister,
                OpeningBalance = request.OpeningBalance ?? 0,
                CurrentBalance = request.OpeningBalance ?? 0,
                Notes = request.Notes,
                IsActive = isActive,
                CreatedAt = DateTime.Now
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, new
            {
                supplier.Id,
                supplier.SupplierName,
                supplier.SupplierCode,
                supplier.ContactPerson,
                supplier.Phone,
                supplier.Mobile,
                supplier.Email,
                supplier.Address,
                supplier.TaxNumber,
                supplier.CommercialRegister,
                supplier.OpeningBalance,
                supplier.CurrentBalance,
                supplier.Notes,
                supplier.IsActive,
                Status = supplier.IsActive ? "active" : "inactive",
                StatusAr = supplier.IsActive ? "نشط" : "موقوف",
                supplier.CreatedAt,
                supplier.UpdatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierRequest request)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound(new { message = "المورد غير موجود" });

            if (!string.IsNullOrWhiteSpace(request.SupplierName))
                supplier.SupplierName = request.SupplierName.Trim();

            supplier.SupplierCode = request.SupplierCode ?? supplier.SupplierCode;
            supplier.ContactPerson = request.ContactPerson ?? supplier.ContactPerson;
            supplier.Phone = request.Phone ?? supplier.Phone;
            supplier.Mobile = request.Mobile ?? supplier.Mobile;
            supplier.Email = request.Email ?? supplier.Email;
            supplier.Address = request.Address ?? supplier.Address;
            supplier.TaxNumber = request.TaxNumber ?? supplier.TaxNumber;
            supplier.CommercialRegister = request.CommercialRegister ?? supplier.CommercialRegister;
            supplier.Notes = request.Notes ?? supplier.Notes;

            if (request.OpeningBalance.HasValue)
                supplier.OpeningBalance = request.OpeningBalance.Value;

            if (request.CurrentBalance.HasValue)
                supplier.CurrentBalance = request.CurrentBalance.Value;

            if (request.IsActive.HasValue || !string.IsNullOrWhiteSpace(request.Status))
                supplier.IsActive = ResolveIsActive(request.IsActive, request.Status, supplier.IsActive);

            supplier.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تحديث المورد",
                supplier = new
                {
                    supplier.Id,
                    supplier.SupplierName,
                    supplier.SupplierCode,
                    supplier.ContactPerson,
                    supplier.Phone,
                    supplier.Mobile,
                    supplier.Email,
                    supplier.Address,
                    supplier.TaxNumber,
                    supplier.CommercialRegister,
                    supplier.OpeningBalance,
                    supplier.CurrentBalance,
                    supplier.Notes,
                    supplier.IsActive,
                    Status = supplier.IsActive ? "active" : "inactive",
                    StatusAr = supplier.IsActive ? "نشط" : "موقوف",
                    supplier.CreatedAt,
                    supplier.UpdatedAt
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound(new { message = "المورد غير موجود" });

            supplier.IsActive = false;
            supplier.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف المورد" });
        }

        private static bool ResolveIsActive(bool? isActive, string? status, bool defaultValue)
        {
            if (isActive.HasValue)
                return isActive.Value;

            if (string.IsNullOrWhiteSpace(status))
                return defaultValue;

            var normalized = status.Trim().ToLowerInvariant();

            if (normalized is "نشط" or "active" or "true" or "1")
                return true;

            if (normalized is "موقوف" or "غير نشط" or "inactive" or "suspended" or "false" or "0")
                return false;

            return defaultValue;
        }
    }
}