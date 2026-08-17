


// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using POS.Data;
// using POS.DTOs;
// using POS.Models;

// namespace POS.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [Authorize]
//     public class ProductsController : ControllerBase
//     {
//         private readonly AppDbContext _context;

//         public ProductsController(AppDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/products
//         [HttpGet]
//         public async Task<IActionResult> GetAll()
//         {
//             var products = await _context.Products
//                 .Include(p => p.Category)
//                 .Include(p => p.Unit)
//                 .Include(p => p.Supplier)
//                 .Include(p => p.ProductWarehouses)   // ← السطر المهم
//                 .Where(p => p.IsActive)
//                 .ToListAsync();

//             return Ok(products);
//         }

//         // GET: api/products/5
//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetById(int id)
//         {
//             var product = await _context.Products
//                 .Include(p => p.Category)
//                 .Include(p => p.Unit)
//                 .Include(p => p.Supplier)
//                 .Include(p => p.ProductWarehouses) // ← أضفناها كمان هون حتى GetById (اللي بينادى بعد الإضافة مباشرة عبر CreatedAtAction) يرجع الكمية صح
//                 .FirstOrDefaultAsync(p => p.Id == id);

//             if (product == null)
//                 return NotFound(new { message = "المنتج غير موجود" });

//             return Ok(product);
//         }

//         // POST: api/products
//         [HttpPost]
//         public async Task<IActionResult> Create([FromBody] ProductDto request)
//         {
//             // نستخدم Transaction صريح حتى نضمن: إما المنتج + صف المخزون ينحفظوا سوا، أو ولا واحد فيهم
//             using var transaction = await _context.Database.BeginTransactionAsync();
//             try
//             {
//                 var product = new Product
//                 {
//                     ProductName = request.ProductName,
//                     ProductCode = request.ProductCode,
//                     Barcode = request.Barcode,
//                     PurchasePrice = request.PurchasePrice,
//                     SellingPrice = request.SellingPrice,
//                     TaxRate = request.TaxRate,
//                     MinStock = request.MinStock,
//                     Description = request.Description,
//                     CategoryId = request.CategoryId,
//                     UnitId = request.UnitId,
//                     SupplierId = request.SupplierId,
//                     IsActive = true,
//                     CreatedAt = DateTime.Now
//                 };

//                 // ✅ نجيب المستودع الرئيسي فعليًا من القاعدة بدل ما نفترض رقم ثابت غلط
//                 // (بمشروعك الحالي المستودع الوحيد معرّفه Id = 2، مش 1 -- تحققنا منها عبر sqlcmd)
//                 var mainWarehouse = await _context.Warehouses.FirstOrDefaultAsync();
//                 if (mainWarehouse == null)
//                 {
//                     await transaction.RollbackAsync();
//                     return BadRequest(new { message = "لا يوجد أي مستودع مسجل بالنظام. الرجاء إنشاء مستودع أولاً." });
//                 }

//                 _context.Products.Add(product);
//                 await _context.SaveChangesAsync(); // لازم نحفظ هون حتى ياخذ product.Id قيمته الحقيقية (لأنه مفتاح أجنبي بـ ProductWarehouse)

//                 // ✅ إنشاء صف المخزون بالمستودع الرئيسي - هاي كانت الخطوة المفقودة
//                 var warehouseStock = new ProductWarehouse
//                 {
//                     ProductId = product.Id,
//                     WarehouseId = mainWarehouse.Id,
//                     Quantity = request.InitialQuantity,
//                     ReservedQuantity = 0,
//                     ReorderPoint = request.MinStock, // نفس الحد الأدنى المدخل بالفورم، منطقي كنقطة بداية
//                 };

//                 _context.ProductWarehouses.Add(warehouseStock);
//                 await _context.SaveChangesAsync();

//                 await transaction.CommitAsync();

//                 return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
//             }
//             catch (Exception)
//             {
//                 await transaction.RollbackAsync();
//                 throw;
//             }
//         }

//         // PUT: api/products/5
//         [HttpPut("{id}")]
//         public async Task<IActionResult> Update(int id, [FromBody] ProductDto request)
//         {
//             var product = await _context.Products.FindAsync(id);

//             if (product == null)
//                 return NotFound(new { message = "المنتج غير موجود" });

//             product.ProductName = request.ProductName;
//             product.ProductCode = request.ProductCode;
//             product.Barcode = request.Barcode;
//             product.PurchasePrice = request.PurchasePrice;
//             product.SellingPrice = request.SellingPrice;
//             product.TaxRate = request.TaxRate;
//             product.MinStock = request.MinStock;
//             product.Description = request.Description;
//             product.CategoryId = request.CategoryId;
//             product.UnitId = request.UnitId;
//             product.SupplierId = request.SupplierId;
//             product.UpdatedAt = DateTime.Now;

//             // ملاحظة: Update ما بيلمس InitialQuantity قصدًا (زي ما هو موثق بالفرونت:
//             // "تعديل الكمية يتم من صفحة إدارة المخزون، وليس من هنا")

//             await _context.SaveChangesAsync();

//             return Ok(product);
//         }

//         // DELETE: api/products/5
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> Delete(int id)
//         {
//             var product = await _context.Products.FindAsync(id);

//             if (product == null)
//                 return NotFound(new { message = "المنتج غير موجود" });

//             // Soft Delete (حذف ناعم)
//             product.IsActive = false;
//             product.UpdatedAt = DateTime.Now;
//             await _context.SaveChangesAsync();

//             return Ok(new { message = "تم حذف المنتج بنجاح" });
//         }
//     }
// }












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
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .Include(p => p.Supplier)
                .Include(p => p.ProductWarehouses)
                .Where(p => p.IsActive)
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .Include(p => p.Supplier)
                .Include(p => p.ProductWarehouses)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound(new { message = "المنتج غير موجود" });

            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = new Product
                {
                    ProductName = request.ProductName,
                    ProductCode = request.ProductCode,
                    Barcode = request.Barcode,
                    PurchasePrice = request.PurchasePrice,
                    SellingPrice = request.SellingPrice,
                    TaxRate = request.TaxRate,
                    MinStock = request.MinStock,
                    Description = request.Description,
                    CategoryId = request.CategoryId,
                    UnitId = request.UnitId,
                    SupplierId = request.SupplierId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var mainWarehouse = await _context.Warehouses.FirstOrDefaultAsync();
                if (mainWarehouse == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "لا يوجد أي مستودع مسجل بالنظام. الرجاء إنشاء مستودع أولاً." });
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                var warehouseStock = new ProductWarehouse
                {
                    ProductId = product.Id,
                    WarehouseId = mainWarehouse.Id,
                    Quantity = request.InitialQuantity,
                    ReservedQuantity = 0,
                    ReorderPoint = request.MinStock,
                };

                _context.ProductWarehouses.Add(warehouseStock);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDto request)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound(new { message = "المنتج غير موجود" });

            product.ProductName = request.ProductName;
            product.ProductCode = request.ProductCode;
            product.Barcode = request.Barcode;
            product.PurchasePrice = request.PurchasePrice;
            product.SellingPrice = request.SellingPrice;
            product.TaxRate = request.TaxRate;
            product.MinStock = request.MinStock;
            product.Description = request.Description;
            product.CategoryId = request.CategoryId;
            product.UnitId = request.UnitId;
            product.SupplierId = request.SupplierId;
            product.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(product);
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound(new { message = "المنتج غير موجود" });

            product.IsActive = false;
            product.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف المنتج بنجاح" });
        }

        // ==========================================================
        // الإضافات الجديدة: حركات المخزون + التعديل اليدوي
        // ==========================================================

        // GET: api/products/5/movements
        [HttpGet("{id}/movements")]
        public async Task<IActionResult> GetProductMovements(int id)
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == id);
            if (!productExists)
                return NotFound(new { message = "المنتج غير موجود" });

            var movements = await _context.StockMovements
                .Where(m => m.ProductId == id)
                .Include(m => m.User)
                .Include(m => m.Supplier)
                .Include(m => m.Customer)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new StockMovementDto
                {
                    Id = m.Id,
                    MovementNumber = m.MovementNumber,
                    MovementType = m.MovementType,
                    Quantity = m.Quantity,
                    UnitPrice = m.UnitPrice,
                    TotalPrice = m.TotalPrice,
                    Notes = m.Notes,
                    CreatedAt = m.CreatedAt,
                    UserName = m.User != null ? m.User.FullName : null,
                    SupplierName = m.Supplier != null ? m.Supplier.SupplierName : null,
                   CustomerName = m.Customer != null ? m.Customer.CustomerName : null
                })
                .ToListAsync();

            return Ok(movements);
        }

        // POST: api/products/5/adjust
        [HttpPost("{id}/adjust")]
        public async Task<IActionResult> AdjustStock(int id, [FromBody] AdjustStockRequestDto dto)
        {
            // نفس منطق Create: نجيب المستودع الرئيسي فعليًا من القاعدة بدل رقم ثابت
            var mainWarehouse = await _context.Warehouses.FirstOrDefaultAsync();
            if (mainWarehouse == null)
                return BadRequest(new { message = "لا يوجد أي مستودع مسجل بالنظام" });

            var productWarehouse = await _context.ProductWarehouses
                .FirstOrDefaultAsync(pw => pw.ProductId == id && pw.WarehouseId == mainWarehouse.Id);

            if (productWarehouse == null)
                return NotFound(new { message = "لا يوجد سجل مخزون لهذا المنتج في هذا المخزن" });

            if (dto.NewQuantity < 0)
                return BadRequest(new { message = "الكمية لا يمكن أن تكون سالبة" });

            var previousQuantity = productWarehouse.Quantity;
            var difference = dto.NewQuantity - previousQuantity;

            if (difference == 0)
                return BadRequest(new { message = "الكمية الجديدة مطابقة للكمية الحالية" });

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;

            var movement = new StockMovement
            {
                MovementNumber = $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}",
                ProductId = id,
                WarehouseId = mainWarehouse.Id,
                MovementType = "جرد",
                Quantity = Math.Abs(difference),
                UnitPrice = 0,
                TotalPrice = 0,
                Notes = dto.Notes ?? $"تعديل يدوي من {previousQuantity} إلى {dto.NewQuantity}",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            productWarehouse.Quantity = dto.NewQuantity;

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            var response = new AdjustStockResponseDto
            {
                ProductId = id,
                PreviousQuantity = previousQuantity,
                NewQuantity = dto.NewQuantity,
                Difference = difference,
                Movement = new StockMovementDto
                {
                    Id = movement.Id,
                    MovementNumber = movement.MovementNumber,
                    MovementType = movement.MovementType,
                    Quantity = movement.Quantity,
                    UnitPrice = movement.UnitPrice,
                    TotalPrice = movement.TotalPrice,
                    Notes = movement.Notes,
                    CreatedAt = movement.CreatedAt
                }
            };

            // TODO: بث SignalR هنا (الخطوة الجاية بعد ما تبعتلي ملف الـ Hub)

            return Ok(response);
        }
    }
}