


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
using System.Security.Claims;

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
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return Unauthorized(new { message = "المستخدم غير موجود" });

            if (!IsAdministrator(currentUser) &&
                (!currentUser.BranchId.HasValue ||
                 !await _context.Branches.AnyAsync(b => b.Id == currentUser.BranchId.Value && b.IsActive)))
                return BadRequest(new { message = "المستخدم غير مرتبط بفرع صالح. يرجى تسجيل الدخول بعد ربطه بفرع." });

            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .Include(p => p.Supplier)
                .Include(p => p.ProductWarehouses)
                .Where(p => p.IsActive)
                .Where(p => IsAdministrator(currentUser) ||
                    p.ProductWarehouses.Any(pw => pw.Warehouse.BranchId == currentUser.BranchId))
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return Unauthorized(new { message = "المستخدم غير موجود" });

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .Include(p => p.Supplier)
                .Include(p => p.ProductWarehouses)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive &&
                    (IsAdministrator(currentUser) ||
                     p.ProductWarehouses.Any(pw => pw.Warehouse.BranchId == currentUser.BranchId)));

            if (product == null)
                return NotFound(new { message = "المنتج غير موجود" });

            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDto request)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return Unauthorized(new { message = "المستخدم غير موجود" });

            var referenceError = await ValidateProductReferencesAsync(request);
            if (referenceError != null)
                return BadRequest(new { message = referenceError });

            if (await _context.Products.AnyAsync(p => p.ProductCode == request.ProductCode))
                return Conflict(new { message = "رمز المنتج مستخدم مسبقاً" });

            if (!string.IsNullOrWhiteSpace(request.Barcode) &&
                await _context.Products.AnyAsync(p => p.Barcode == request.Barcode))
                return Conflict(new { message = "الباركود مستخدم مسبقاً" });

            var mainWarehouse = await GetUserWarehouseAsync(currentUser, request.WarehouseId);
            if (mainWarehouse == null)
            {
                var message = request.WarehouseId.HasValue
                    ? "المستودع المحدد غير موجود أو غير مرتبط بفرع المستخدم الحالي"
                    : "لا يوجد مستودع نشط مرتبط بفرع المستخدم الحالي. يرجى إنشاء المستودع ثم إعادة تسجيل الدخول.";
                return NotFound(new { message });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
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

                var createdProduct = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Unit)
                    .Include(p => p.Supplier)
                    .Include(p => p.ProductWarehouses)
                    .FirstAsync(p => p.Id == product.Id);

                return CreatedAtAction(nameof(GetById), new { id = product.Id }, createdProduct);
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
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return Unauthorized(new { message = "المستخدم غير موجود" });

            var product = await _context.Products
                .Include(p => p.ProductWarehouses)
                    .ThenInclude(pw => pw.Warehouse)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive &&
                    (IsAdministrator(currentUser) ||
                     p.ProductWarehouses.Any(pw => pw.Warehouse.BranchId == currentUser.BranchId)));

            if (product == null)
                return NotFound(new { message = "المنتج غير موجود" });

            var referenceError = await ValidateProductReferencesAsync(request);
            if (referenceError != null)
                return BadRequest(new { message = referenceError });

            if (await _context.Products.AnyAsync(p => p.Id != id && p.ProductCode == request.ProductCode))
                return Conflict(new { message = "رمز المنتج مستخدم مسبقاً" });

            if (!string.IsNullOrWhiteSpace(request.Barcode) &&
                await _context.Products.AnyAsync(p => p.Id != id && p.Barcode == request.Barcode))
                return Conflict(new { message = "الباركود مستخدم مسبقاً" });

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
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return Unauthorized(new { message = "المستخدم غير موجود" });

            var product = await _context.Products
                .Include(p => p.ProductWarehouses)
                    .ThenInclude(pw => pw.Warehouse)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive &&
                    (IsAdministrator(currentUser) ||
                     p.ProductWarehouses.Any(pw => pw.Warehouse.BranchId == currentUser.BranchId)));

            if (product == null)
                return NotFound(new { message = "المنتج غير موجود" });

            product.IsActive = false;
            product.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف المنتج بنجاح" });
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out var id))
                return null;

            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }

        private static bool IsAdministrator(User user) =>
            user.Role?.RoleName is "Admin" or "مدير النظام" or "Administrator";

        private async Task<Warehouse?> GetUserWarehouseAsync(User user, int? warehouseId = null)
        {
            if (!IsAdministrator(user) &&
                (!user.BranchId.HasValue ||
                 !await _context.Branches.AnyAsync(b => b.Id == user.BranchId.Value && b.IsActive)))
                return null;

            var warehouses = _context.Warehouses.Where(w => w.IsActive);

            if (!IsAdministrator(user))
                warehouses = warehouses.Where(w => w.BranchId == user.BranchId);

            if (warehouseId.HasValue)
                return await warehouses.FirstOrDefaultAsync(w => w.Id == warehouseId.Value);

            return await warehouses
                .OrderByDescending(w => w.IsMainWarehouse)
                .ThenBy(w => w.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<string?> ValidateProductReferencesAsync(ProductDto request)
        {
            if (!await _context.Units.AnyAsync(u => u.Id == request.UnitId && u.IsActive))
                return "الوحدة غير موجودة أو غير مفعلة";

            if (request.CategoryId.HasValue &&
                !await _context.Categories.AnyAsync(c => c.Id == request.CategoryId.Value && c.IsActive))
                return "التصنيف غير موجود أو غير مفعل";

            if (request.SupplierId.HasValue &&
                !await _context.Suppliers.AnyAsync(s => s.Id == request.SupplierId.Value && s.IsActive))
                return "المورد غير موجود أو غير مفعل";

            return null;
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
        [HttpPost("products/{id}/adjust")]
        public async Task<IActionResult> AdjustStock(int id, [FromBody] AdjustStockRequestDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "البيانات المرسلة غير صالحة" });

            var productExists = await _context.Products.AnyAsync(p => p.Id == id);
            if (!productExists)
                return NotFound(new { message = "المنتج غير موجود" });

            var mainWarehouse = await _context.Warehouses.FirstOrDefaultAsync();
            if (mainWarehouse == null)
                return BadRequest(new { message = "لا يوجد أي مستودع مسجل بالنظام" });

            var warehouseId = dto.WarehouseId > 0 ? dto.WarehouseId : mainWarehouse.Id;
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == warehouseId);
            if (warehouse == null)
                return NotFound(new { message = "المستودع غير موجود" });

            var productWarehouse = await _context.ProductWarehouses
                .FirstOrDefaultAsync(pw => pw.ProductId == id && pw.WarehouseId == warehouseId);

            if (productWarehouse == null)
            {
                productWarehouse = new ProductWarehouse
                {
                    ProductId = id,
                    WarehouseId = warehouseId,
                    Quantity = 0,
                    ReservedQuantity = 0,
                    ReorderPoint = 0
                };

                _context.ProductWarehouses.Add(productWarehouse);
                await _context.SaveChangesAsync();
            }

            var adjustmentType = string.IsNullOrWhiteSpace(dto.Type) ? "add" : dto.Type.Trim().ToLowerInvariant();
            var baseQuantity = dto.Quantity ?? dto.NewQuantity ?? 0;

            if (baseQuantity < 0)
                return BadRequest(new { message = "الكمية لا يمكن أن تكون سالبة" });

            int newQuantity;

            switch (adjustmentType)
            {
                case "add":
                    newQuantity = productWarehouse.Quantity + baseQuantity;
                    break;

                case "subtract":
                    newQuantity = Math.Max(0, productWarehouse.Quantity - baseQuantity);
                    break;

                case "set":
                    if (!dto.NewQuantity.HasValue)
                        return BadRequest(new { message = "يجب إرسال NewQuantity عند استخدام النوع set" });

                    newQuantity = dto.NewQuantity.Value;
                    if (newQuantity < 0)
                        return BadRequest(new { message = "الكمية لا يمكن أن تكون سالبة" });
                    break;

                default:
                    return BadRequest(new { message = "نوع التعديل غير صالح. استخدم add أو subtract أو set." });
            }

            var previousQuantity = productWarehouse.Quantity;
            var difference = newQuantity - previousQuantity;

            if (difference == 0)
                return BadRequest(new { message = "الكمية الجديدة مطابقة للكمية الحالية" });

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;

            var movement = new StockMovement
            {
                MovementNumber = $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}",
                ProductId = id,
                WarehouseId = warehouseId,
                MovementType = adjustmentType == "subtract" ? "إخراج" : adjustmentType == "set" ? "جرد" : "استلام",
                Quantity = Math.Abs(difference),
                UnitPrice = 0,
                TotalPrice = 0,
                Notes = dto.Notes ?? dto.Reason ?? $"تعديل يدوي من {previousQuantity} إلى {newQuantity}",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            productWarehouse.Quantity = newQuantity;

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            var response = new AdjustStockResponseDto
            {
                ProductId = id,
                PreviousQuantity = previousQuantity,
                NewQuantity = newQuantity,
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

        // POST: api/products/5/restock
        [HttpPost("{id}/restock")]
        [HttpPost("Products/{id}/restock")]
        public async Task<IActionResult> RestockStock(int id, [FromBody] AdjustStockRequestDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "البيانات المرسلة غير صالحة" });

            dto.Type = "add";
            dto.Notes ??= dto.Reason ?? "استلام بضاعة";

            if (!dto.Quantity.HasValue && dto.NewQuantity.HasValue)
                dto.Quantity = dto.NewQuantity;

            if (!dto.Quantity.HasValue || dto.Quantity.Value <= 0)
                return BadRequest(new { message = "يجب إرسال Quantity أكبر من صفر" });

            return await AdjustStock(id, dto);
        }
    }
}