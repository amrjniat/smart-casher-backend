// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using POS.Data;
// using POS.Models;
// using System.Security.Claims;

// namespace POS.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [Authorize]
//     public class WarehouseController : ControllerBase
//     {
//         private readonly AppDbContext _context;

//         public WarehouseController(AppDbContext context)
//         {
//             _context = context;
//         }

//         // ==================== 📊 لوحة التحكم (Dashboard) ====================

//         // GET: api/warehouse/stats
//         [HttpGet("stats")]
//         public async Task<IActionResult> GetStats()
//         {
//             var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
//             var totalStock = await _context.ProductWarehouses.SumAsync(pw => pw.Quantity);

//             var totalEntries = await _context.StockMovements
//                 .CountAsync(m => m.MovementType == "إدخال");

//             var totalExits = await _context.StockMovements
//                 .CountAsync(m => m.MovementType == "إخراج");

//             var lowStockItems = await _context.ProductWarehouses
//                 .Where(pw => pw.Quantity < pw.ReorderPoint && pw.Quantity > 0)
//                 .CountAsync();

//             var outOfStock = await _context.ProductWarehouses
//                 .Where(pw => pw.Quantity == 0)
//                 .CountAsync();

//             return Ok(new
//             {
//                 totalProducts,
//                 totalStock,
//                 totalEntries,
//                 totalExits,
//                 lowStockItems,
//                 outOfStock
//             });
//         }

//         // GET: api/warehouse/stats/categories
//         [HttpGet("stats/categories")]
//         public async Task<IActionResult> GetCategoriesStatus()
//         {
//             var categories = await _context.Categories
//                 .Include(c => c.Products)
//                 .ThenInclude(p => p.ProductWarehouses)
//                 .Where(c => c.IsActive)
//                 .Select(c => new
//                 {
//                     c.Id,
//                     c.CategoryName,
//                     TotalQuantity = c.Products
//                         .SelectMany(p => p.ProductWarehouses)
//                         .Sum(pw => pw.Quantity),
//                     ProductCount = c.Products.Count(p => p.IsActive),
//                     c.Description
//                 })
//                 .ToListAsync();

//             return Ok(categories);
//         }

//         // ==================== 📋 الحركات (Movements) ====================

//         // GET: api/warehouse/movements/recent
//         [HttpGet("movements/recent")]
//         public async Task<IActionResult> GetRecentMovements([FromQuery] int count = 10)
//         {
//             var movements = await _context.StockMovements
//                 .Include(m => m.Product)
//                 .Include(m => m.Warehouse)
//                 .OrderByDescending(m => m.CreatedAt)
//                 .Take(count)
//                 .Select(m => new
//                 {
//                     m.Id,
//                     m.MovementNumber,
//                     m.MovementType,
//                     m.Quantity,
//                     ProductName = m.Product.ProductName,
//                     ProductCode = m.Product.ProductCode,
//                     WarehouseName = m.Warehouse.WarehouseName,
//                     m.UnitPrice,
//                     m.TotalPrice,
//                     m.CreatedAt,
//                     m.Notes
//                 })
//                 .ToListAsync();

//             return Ok(movements);
//         }

//         // GET: api/warehouse/movements (مع فلترة)
//         [HttpGet("movements")]
//         public async Task<IActionResult> GetAllMovements(
//             [FromQuery] string? type,
//             [FromQuery] int? productId,
//             [FromQuery] int? warehouseId,
//             [FromQuery] DateTime? from,
//             [FromQuery] DateTime? to,
//             [FromQuery] int page = 1,
//             [FromQuery] int pageSize = 20)
//         {
//             var query = _context.StockMovements
//                 .Include(m => m.Product)
//                 .Include(m => m.Warehouse)
//                 .AsQueryable();

//             if (!string.IsNullOrEmpty(type))
//                 query = query.Where(m => m.MovementType == type);

//             if (productId.HasValue)
//                 query = query.Where(m => m.ProductId == productId.Value);

//             if (warehouseId.HasValue)
//                 query = query.Where(m => m.WarehouseId == warehouseId.Value);

//             if (from.HasValue)
//                 query = query.Where(m => m.CreatedAt >= from.Value);

//             if (to.HasValue)
//                 query = query.Where(m => m.CreatedAt <= to.Value);

//             var total = await query.CountAsync();

//             var movements = await query
//                 .OrderByDescending(m => m.CreatedAt)
//                 .Skip((page - 1) * pageSize)
//                 .Take(pageSize)
//                 .Select(m => new
//                 {
//                     m.Id,
//                     m.MovementNumber,
//                     m.MovementType,
//                     m.Quantity,
//                     ProductName = m.Product.ProductName,
//                     ProductCode = m.Product.ProductCode,
//                     WarehouseName = m.Warehouse.WarehouseName,
//                     m.UnitPrice,
//                     m.TotalPrice,
//                     m.CreatedAt,
//                     m.Notes
//                 })
//                 .ToListAsync();

//             return Ok(new
//             {
//                 total,
//                 page,
//                 pageSize,
//                 movements
//             });
//         }

//         // ==================== 🔔 التنبيهات (Alerts) ====================

//         // GET: api/warehouse/alerts
//         [HttpGet("alerts")]
//         public async Task<IActionResult> GetAlerts([FromQuery] bool onlyUnread = true)
//         {
//             var query = _context.StockAlerts
//                 .Include(a => a.Product)
//                 .Include(a => a.Warehouse)
//                 .Where(a => a.IsActive);

//             if (onlyUnread)
//                 query = query.Where(a => !a.IsRead);

//             var alerts = await query
//                 .OrderByDescending(a => a.CreatedAt)
//                 .Select(a => new
//                 {
//                     a.Id,
//                     a.AlertType,
//                     a.Message,
//                     ProductName = a.Product.ProductName,
//                     ProductCode = a.Product.ProductCode,
//                     WarehouseName = a.Warehouse.WarehouseName,
//                     a.IsRead,
//                     a.CreatedAt
//                 })
//                 .ToListAsync();

//             return Ok(alerts);
//         }

//         // PUT: api/warehouse/alerts/{id}/read
//         [HttpPut("alerts/{id}/read")]
//         public async Task<IActionResult> MarkAlertAsRead(int id)
//         {
//             var alert = await _context.StockAlerts.FindAsync(id);
//             if (alert == null)
//                 return NotFound(new { message = "التنبيه غير موجود" });

//             alert.IsRead = true;
//             alert.ReadAt = DateTime.Now;
//             await _context.SaveChangesAsync();

//             return Ok(new { message = "تم تحديث التنبيه" });
//         }

//         // ==================== 📥 استلام بضاعة (Receive) ====================

//         // POST: api/warehouse/receive
//         [HttpPost("receive")]
//         public async Task<IActionResult> ReceiveStock([FromBody] ReceiveStockRequest request)
//         {
//             var product = await _context.Products.FindAsync(request.ProductId);
//             if (product == null)
//                 return NotFound(new { message = "المنتج غير موجود" });

//             var warehouse = await _context.Warehouses.FindAsync(request.WarehouseId);
//             if (warehouse == null)
//                 return NotFound(new { message = "المستودع غير موجود" });

//             // تسجيل الحركة
//             var movement = new StockMovement
//             {
//                 MovementNumber = GenerateMovementNumber("IN"),
//                 ProductId = request.ProductId,
//                 WarehouseId = request.WarehouseId,
//                 MovementType = "إدخال",
//                 Quantity = request.Quantity,
//                 UnitPrice = request.UnitPrice ?? product.PurchasePrice,
//                 TotalPrice = request.Quantity * (request.UnitPrice ?? product.PurchasePrice),
//                 Notes = request.Notes,
//                 SupplierId = request.SupplierId,
//                 UserId = GetCurrentUserId(),
//                 CreatedAt = DateTime.Now
//             };

//             // تحديث المخزون
//             var productWarehouse = await _context.ProductWarehouses
//                 .FirstOrDefaultAsync(pw => pw.ProductId == request.ProductId && pw.WarehouseId == request.WarehouseId);

//             if (productWarehouse == null)
//             {
//                 productWarehouse = new ProductWarehouse
//                 {
//                     ProductId = request.ProductId,
//                     WarehouseId = request.WarehouseId,
//                     Quantity = request.Quantity,
//                     ReorderPoint = product.MinStock
//                 };
//                 _context.ProductWarehouses.Add(productWarehouse);
//             }
//             else
//             {
//                 productWarehouse.Quantity += request.Quantity;
//             }

//             _context.StockMovements.Add(movement);
//             await _context.SaveChangesAsync();

//             // التحقق من التنبيهات بعد التحديث
//             await CheckAndCreateAlerts(request.ProductId, request.WarehouseId);

//             return Ok(new { message = "تم استلام البضاعة بنجاح", movement });
//         }

//         // ==================== 📤 إخراج بضاعة (Exit) ====================

//         // POST: api/warehouse/exit
//         [HttpPost("exit")]
//         public async Task<IActionResult> ExitStock([FromBody] ExitStockRequest request)
//         {
//             var product = await _context.Products.FindAsync(request.ProductId);
//             if (product == null)
//                 return NotFound(new { message = "المنتج غير موجود" });

//             var productWarehouse = await _context.ProductWarehouses
//                 .FirstOrDefaultAsync(pw => pw.ProductId == request.ProductId && pw.WarehouseId == request.WarehouseId);

//             if (productWarehouse == null || productWarehouse.Quantity < request.Quantity)
//                 return BadRequest(new { message = "الكمية غير متوفرة في المستودع" });

//             // تسجيل الحركة
//             var movement = new StockMovement
//             {
//                 MovementNumber = GenerateMovementNumber("OUT"),
//                 ProductId = request.ProductId,
//                 WarehouseId = request.WarehouseId,
//                 MovementType = "إخراج",
//                 Quantity = request.Quantity,
//                 UnitPrice = request.UnitPrice ?? product.SellingPrice,
//                 TotalPrice = request.Quantity * (request.UnitPrice ?? product.SellingPrice),
//                 Notes = request.Notes,
//                 CustomerId = request.CustomerId,
//                 UserId = GetCurrentUserId(),
//                 CreatedAt = DateTime.Now
//             };

//             // تحديث المخزون
//             productWarehouse.Quantity -= request.Quantity;

//             _context.StockMovements.Add(movement);
//             await _context.SaveChangesAsync();

//             // التحقق من التنبيهات بعد التحديث
//             await CheckAndCreateAlerts(request.ProductId, request.WarehouseId);

//             return Ok(new { message = "تم إخراج البضاعة بنجاح", movement });
//         }

//         // ==================== 🔄 نقل مخزون (Transfer) ====================

//         // POST: api/warehouse/transfer
//         [HttpPost("transfer")]
//         public async Task<IActionResult> TransferStock([FromBody] TransferStockRequest request)
//         {
//             var product = await _context.Products.FindAsync(request.ProductId);
//             if (product == null)
//                 return NotFound(new { message = "المنتج غير موجود" });

//             var fromWarehouse = await _context.ProductWarehouses
//                 .FirstOrDefaultAsync(pw => pw.ProductId == request.ProductId && pw.WarehouseId == request.FromWarehouseId);

//             if (fromWarehouse == null || fromWarehouse.Quantity < request.Quantity)
//                 return BadRequest(new { message = "الكمية غير متوفرة في المستودع المصدر" });

//             var toWarehouse = await _context.ProductWarehouses
//                 .FirstOrDefaultAsync(pw => pw.ProductId == request.ProductId && pw.WarehouseId == request.ToWarehouseId);

//             // حركة إخراج من المصدر
//             var exitMovement = new StockMovement
//             {
//                 MovementNumber = GenerateMovementNumber("TRF-OUT"),
//                 ProductId = request.ProductId,
//                 WarehouseId = request.FromWarehouseId,
//                 MovementType = "إخراج",
//                 Quantity = request.Quantity,
//                 UnitPrice = product.PurchasePrice,
//                 TotalPrice = request.Quantity * product.PurchasePrice,
//                 Notes = $"نقل إلى مستودع {request.ToWarehouseId}",
//                 UserId = GetCurrentUserId(),
//                 CreatedAt = DateTime.Now
//             };

//             // حركة إدخال إلى الهدف
//             var entryMovement = new StockMovement
//             {
//                 MovementNumber = GenerateMovementNumber("TRF-IN"),
//                 ProductId = request.ProductId,
//                 WarehouseId = request.ToWarehouseId,
//                 MovementType = "إدخال",
//                 Quantity = request.Quantity,
//                 UnitPrice = product.PurchasePrice,
//                 TotalPrice = request.Quantity * product.PurchasePrice,
//                 Notes = $"نقل من مستودع {request.FromWarehouseId}",
//                 UserId = GetCurrentUserId(),
//                 CreatedAt = DateTime.Now
//             };

//             // تحديث الكميات
//             fromWarehouse.Quantity -= request.Quantity;

//             if (toWarehouse == null)
//             {
//                 toWarehouse = new ProductWarehouse
//                 {
//                     ProductId = request.ProductId,
//                     WarehouseId = request.ToWarehouseId,
//                     Quantity = request.Quantity,
//                     ReorderPoint = product.MinStock
//                 };
//                 _context.ProductWarehouses.Add(toWarehouse);
//             }
//             else
//             {
//                 toWarehouse.Quantity += request.Quantity;
//             }

//             // تسجيل عملية النقل
//             var transfer = new StockTransfer
//             {
//                 TransferNumber = GenerateMovementNumber("TRF"),
//                 ProductId = request.ProductId,
//                 FromWarehouseId = request.FromWarehouseId,
//                 ToWarehouseId = request.ToWarehouseId,
//                 Quantity = request.Quantity,
//                 Notes = request.Notes,
//                 UserId = GetCurrentUserId(),
//                 CompletedAt = DateTime.Now,
//                 CreatedAt = DateTime.Now
//             };

//             _context.StockMovements.Add(exitMovement);
//             _context.StockMovements.Add(entryMovement);
//             _context.StockTransfers.Add(transfer);
//             await _context.SaveChangesAsync();

//             // التحقق من التنبيهات
//             await CheckAndCreateAlerts(request.ProductId, request.FromWarehouseId);
//             await CheckAndCreateAlerts(request.ProductId, request.ToWarehouseId);

//             return Ok(new { message = "تم نقل المخزون بنجاح", transfer });
//         }

//         // ==================== 📋 جرد سريع (Quick Count) ====================

//         // POST: api/warehouse/quick-count
//         [HttpPost("quick-count")]
//         public async Task<IActionResult> QuickCount([FromBody] QuickCountRequest request)
//         {
//             var productWarehouse = await _context.ProductWarehouses
//                 .FirstOrDefaultAsync(pw => pw.ProductId == request.ProductId && pw.WarehouseId == request.WarehouseId);

//             if (productWarehouse == null)
//                 return NotFound(new { message = "المنتج غير موجود في المستودع" });

//             // تسجيل حركة جرد
//             var movement = new StockMovement
//             {
//                 MovementNumber = GenerateMovementNumber("QC"),
//                 ProductId = request.ProductId,
//                 WarehouseId = request.WarehouseId,
//                 MovementType = "جرد",
//                 Quantity = request.CountedQuantity,
//                 Notes = $"جرد سريع - الكمية المسجلة: {productWarehouse.Quantity}، الكمية المعدودة: {request.CountedQuantity}",
//                 UserId = GetCurrentUserId(),
//                 CreatedAt = DateTime.Now
//             };

//             _context.StockMovements.Add(movement);

//             // تحديث الكمية إذا اختلفت
//             if (productWarehouse.Quantity != request.CountedQuantity)
//             {
//                 productWarehouse.Quantity = request.CountedQuantity;
//             }

//             await _context.SaveChangesAsync();

//             // التحقق من التنبيهات
//             await CheckAndCreateAlerts(request.ProductId, request.WarehouseId);

//             return Ok(new { message = "تم تسجيل الجرد بنجاح", movement });
//         }

//         // ==================== 🛠️ دوال مساعدة (Helpers) ====================

//         private string GenerateMovementNumber(string prefix)
//         {
//             var date = DateTime.Now.ToString("yyMMdd");
//             var count = _context.StockMovements.Count() + 1;
//             return $"{prefix}-{date}-{count:D4}";
//         }

//         private int GetCurrentUserId()
//         {
//             var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//             return userId != null ? int.Parse(userId) : 0;
//         }

//         private async Task CheckAndCreateAlerts(int productId, int warehouseId)
//         {
//             var productWarehouse = await _context.ProductWarehouses
//                 .Include(pw => pw.Product)
//                 .FirstOrDefaultAsync(pw => pw.ProductId == productId && pw.WarehouseId == warehouseId);

//             if (productWarehouse == null) return;

//             // التحقق من وجود تنبيه مسبق لنفس المنتج والمستودع
//             var existingAlert = await _context.StockAlerts
//                 .FirstOrDefaultAsync(a => a.ProductId == productId && a.WarehouseId == warehouseId && !a.IsRead);

//             // إذا كانت الكمية 0
//             if (productWarehouse.Quantity == 0)
//             {
//                 var message = $"نفذت كمية \"{productWarehouse.Product.ProductName}\"";

//                 if (existingAlert == null)
//                 {
//                     var alert = new StockAlert
//                     {
//                         ProductId = productId,
//                         WarehouseId = warehouseId,
//                         AlertType = "OutOfStock",
//                         Message = message,
//                         IsActive = true,
//                         CreatedAt = DateTime.Now
//                     };
//                     _context.StockAlerts.Add(alert);
//                 }
//                 else if (existingAlert.AlertType != "OutOfStock")
//                 {
//                     existingAlert.AlertType = "OutOfStock";
//                     existingAlert.Message = message;
//                 }
//             }
//             // إذا كانت الكمية أقل من الحد الأدنى
//             else if (productWarehouse.Quantity < productWarehouse.ReorderPoint)
//             {
//                 var message = $"مخزون \"{productWarehouse.Product.ProductName}\" أقل من الحد الأدنى";

//                 if (existingAlert == null)
//                 {
//                     var alert = new StockAlert
//                     {
//                         ProductId = productId,
//                         WarehouseId = warehouseId,
//                         AlertType = "LowStock",
//                         Message = message,
//                         IsActive = true,
//                         CreatedAt = DateTime.Now
//                     };
//                     _context.StockAlerts.Add(alert);
//                 }
//                 else if (existingAlert.AlertType != "LowStock")
//                 {
//                     existingAlert.AlertType = "LowStock";
//                     existingAlert.Message = message;
//                 }
//             }
//             // إذا كانت الكمية طبيعية، نغلق التنبيه المفتوح
//             else
//             {
//                 if (existingAlert != null)
//                 {
//                     existingAlert.IsRead = true;
//                     existingAlert.ReadAt = DateTime.Now;
//                 }
//             }

//             await _context.SaveChangesAsync();
//         }
//     }

//     // ==================== 📦 DTOs (نماذج البيانات) ====================

//     public class ReceiveStockRequest
//     {
//         public int ProductId { get; set; }
//         public int WarehouseId { get; set; }
//         public int Quantity { get; set; }
//         public decimal? UnitPrice { get; set; }
//         public string? Notes { get; set; }
//         public int? SupplierId { get; set; }
//     }

//     public class ExitStockRequest
//     {
//         public int ProductId { get; set; }
//         public int WarehouseId { get; set; }
//         public int Quantity { get; set; }
//         public decimal? UnitPrice { get; set; }
//         public string? Notes { get; set; }
//         public int? CustomerId { get; set; }
//     }

//     public class TransferStockRequest
//     {
//         public int ProductId { get; set; }
//         public int FromWarehouseId { get; set; }
//         public int ToWarehouseId { get; set; }
//         public int Quantity { get; set; }
//         public string? Notes { get; set; }
//     }

//     public class QuickCountRequest
//     {
//         public int ProductId { get; set; }
//         public int WarehouseId { get; set; }
//         public int CountedQuantity { get; set; }
//     }
// }




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
    public class WarehouseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WarehouseController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== 📊 لوحة التحكم (Dashboard) ====================

        // GET: api/warehouse/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
            var totalStock = await _context.ProductWarehouses.SumAsync(pw => pw.Quantity);

            var totalEntries = await _context.StockMovements
                .CountAsync(m => m.MovementType == "إدخال");

            var totalExits = await _context.StockMovements
                .CountAsync(m => m.MovementType == "إخراج");

            var lowStockItems = await _context.ProductWarehouses
                .Where(pw => pw.Quantity < pw.ReorderPoint && pw.Quantity > 0)
                .CountAsync();

            var outOfStock = await _context.ProductWarehouses
                .Where(pw => pw.Quantity == 0)
                .CountAsync();

            return Ok(new
            {
                totalProducts,
                totalStock,
                totalEntries,
                totalExits,
                lowStockItems,
                outOfStock
            });
        }

        // GET: api/warehouse/stats/categories
        [HttpGet("stats/categories")]
        public async Task<IActionResult> GetCategoriesStatus()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .ThenInclude(p => p.ProductWarehouses)
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.Id,
                    c.CategoryName,
                    TotalQuantity = c.Products
                        .SelectMany(p => p.ProductWarehouses)
                        .Sum(pw => pw.Quantity),
                    ProductCount = c.Products.Count(p => p.IsActive),
                    c.Description
                })
                .ToListAsync();

            return Ok(categories);
        }

        // ==================== 📈 ملخص الحركة الشهري (جديد) ====================

        // GET: api/warehouse/movements/monthly-summary
        [HttpGet("movements/monthly-summary")]
        public async Task<IActionResult> GetMonthlySummary([FromQuery] int months = 6)
        {
            var startDate = DateTime.Now.AddMonths(-(months - 1));
            startDate = new DateTime(startDate.Year, startDate.Month, 1);

            var movements = await _context.StockMovements
                .Where(m => m.CreatedAt >= startDate &&
                            (m.MovementType == "إدخال" || m.MovementType == "إخراج"))
                .Select(m => new { m.CreatedAt, m.MovementType, m.Quantity })
                .ToListAsync();

            var arabicMonths = new Dictionary<int, string>
            {
                {1,"يناير"},{2,"فبراير"},{3,"مارس"},{4,"أبريل"},{5,"مايو"},{6,"يونيو"},
                {7,"يوليو"},{8,"أغسطس"},{9,"سبتمبر"},{10,"أكتوبر"},{11,"نوفمبر"},{12,"ديسمبر"}
            };

            var result = new List<object>();
            for (int i = 0; i < months; i++)
            {
                var monthDate = startDate.AddMonths(i);
                var monthMovements = movements.Where(m =>
                    m.CreatedAt.Year == monthDate.Year && m.CreatedAt.Month == monthDate.Month);

                result.Add(new
                {
                    month = monthDate.ToString("yyyy-MM"),
                    monthName = arabicMonths[monthDate.Month],
                    inbound = monthMovements.Where(m => m.MovementType == "إدخال").Sum(m => m.Quantity),
                    outbound = monthMovements.Where(m => m.MovementType == "إخراج").Sum(m => m.Quantity)
                });
            }

            return Ok(result);
        }

        // ==================== 🔥 المنتجات الأكثر حركة (جديد) ====================

        // GET: api/warehouse/products/most-active
        [HttpGet("products/most-active")]
        public async Task<IActionResult> GetMostActiveProducts(
            [FromQuery] int count = 5,
            [FromQuery] string period = "month")
        {
            DateTime fromDate = period switch
            {
                "week" => DateTime.Now.AddDays(-7),
                "year" => DateTime.Now.AddYears(-1),
                _ => DateTime.Now.AddMonths(-1)
            };

            var result = await _context.StockMovements
                .Where(m => m.MovementType == "إخراج" && m.CreatedAt >= fromDate)
                .GroupBy(m => new { m.ProductId, m.Product.ProductName })
                .Select(g => new
                {
                    productId = g.Key.ProductId,
                    productName = g.Key.ProductName,
                    totalQuantity = g.Sum(m => m.Quantity)
                })
                .OrderByDescending(x => x.totalQuantity)
                .Take(count)
                .ToListAsync();

            return Ok(result);
        }

        // ==================== 📉 تطور إجمالي المخزون الأسبوعي (جديد) ====================

        // GET: api/warehouse/stats/inventory-evolution
        // ملاحظة: حساب تقريبي بالاعتماد على صافي حركات الإدخال/الإخراج تراكمياً
        // (ما في جدول Snapshot تاريخي حالياً). عمليات "جرد" غير محسوبة هون.
        [HttpGet("stats/inventory-evolution")]
        public async Task<IActionResult> GetInventoryEvolution([FromQuery] int weeks = 4)
        {
            var startDate = DateTime.Now.AddDays(-7 * weeks);

            var allMovements = await _context.StockMovements
                .Where(m => m.MovementType == "إدخال" || m.MovementType == "إخراج")
                .Select(m => new { m.CreatedAt, m.MovementType, m.Quantity })
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            // الرصيد الافتتاحي = صافي كل الحركات قبل بداية النافذة الزمنية
            long runningTotal = allMovements
                .Where(m => m.CreatedAt < startDate)
                .Sum(m => m.MovementType == "إدخال" ? m.Quantity : -m.Quantity);

            var result = new List<object>();
            for (int i = 0; i < weeks; i++)
            {
                var weekStart = startDate.AddDays(7 * i);
                var weekEnd = weekStart.AddDays(7);

                var weekMovements = allMovements.Where(m => m.CreatedAt >= weekStart && m.CreatedAt < weekEnd);
                runningTotal += weekMovements.Sum(m => m.MovementType == "إدخال" ? m.Quantity : -m.Quantity);

                result.Add(new
                {
                    weekLabel = $"الأسبوع {i + 1}",
                    totalQuantity = runningTotal
                });
            }

            return Ok(result);
        }

        // ==================== ⚠️ قائمة المنتجات منخفضة المخزون (جديد) ====================

        // GET: api/warehouse/products/low-stock
        [HttpGet("products/low-stock")]
        public async Task<IActionResult> GetLowStockProductsList()
        {
            var result = await _context.ProductWarehouses
                .Include(pw => pw.Product)
                .Where(pw => pw.Quantity < pw.ReorderPoint && pw.Quantity > 0)
                .Select(pw => new
                {
                    productId = pw.ProductId,
                    productName = pw.Product.ProductName,
                    currentQuantity = pw.Quantity,
                    minStock = pw.ReorderPoint
                })
                .ToListAsync();

            return Ok(result);
        }

        // ==================== ❌ قائمة المنتجات النافدة (جديد) ====================

        // GET: api/warehouse/products/out-of-stock
        [HttpGet("products/out-of-stock")]
        public async Task<IActionResult> GetOutOfStockProductsList()
        {
            var outOfStockItems = await _context.ProductWarehouses
                .Include(pw => pw.Product)
                .Where(pw => pw.Quantity == 0)
                .ToListAsync();

            var result = new List<object>();
            foreach (var item in outOfStockItems)
            {
                var lastMovement = await _context.StockMovements
                    .Where(m => m.ProductId == item.ProductId)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefaultAsync();

                result.Add(new
                {
                    productId = item.ProductId,
                    productName = item.Product.ProductName,
                    lastMovementDate = lastMovement?.CreatedAt,
                    lastMovementType = lastMovement?.MovementType
                });
            }

            return Ok(result);
        }

        // ==================== 📊 مؤشرات الأداء KPIs (جديد) ====================

        // GET: api/warehouse/stats/kpis
        [HttpGet("stats/kpis")]
        public async Task<IActionResult> GetKpis()
        {
            var monthAgo = DateTime.Now.AddDays(-30);
            var today = DateTime.Now.Date;

            var totalInboundMonth = await _context.StockMovements
                .Where(m => m.MovementType == "إدخال" && m.CreatedAt >= monthAgo)
                .SumAsync(m => m.Quantity);

            var totalOutboundMonth = await _context.StockMovements
                .Where(m => m.MovementType == "إخراج" && m.CreatedAt >= monthAgo)
                .SumAsync(m => m.Quantity);

            var averageStock = await _context.ProductWarehouses
                .AverageAsync(pw => (double?)pw.Quantity) ?? 0;

            var todayMovementsCount = await _context.StockMovements
                .CountAsync(m => m.CreatedAt.Date == today);

            var turnoverRate = averageStock > 0
                ? Math.Round(totalOutboundMonth / averageStock, 2)
                : 0;

            var avgDailyConsumption = Math.Round(totalOutboundMonth / 30.0, 1);
            var avgDailyInbound = Math.Round(totalInboundMonth / 30.0, 1);

            return Ok(new
            {
                turnoverRate,
                avgDailyConsumption,
                todayMovementsCount,
                avgDailyInbound
            });
        }

        // ==================== 📋 الحركات (Movements) ====================

        // GET: api/warehouse/movements/recent
        [HttpGet("movements/recent")]
        public async Task<IActionResult> GetRecentMovements([FromQuery] int count = 10)
        {
            var movements = await _context.StockMovements
                .Include(m => m.Product)
                .Include(m => m.Warehouse)
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .Select(m => new
                {
                    m.Id,
                    m.MovementNumber,
                    m.MovementType,
                    m.Quantity,
                    ProductName = m.Product.ProductName,
                    ProductCode = m.Product.ProductCode,
                    WarehouseName = m.Warehouse.WarehouseName,
                    m.UnitPrice,
                    m.TotalPrice,
                    m.CreatedAt,
                    m.Notes
                })
                .ToListAsync();

            return Ok(movements);
        }

        // GET: api/warehouse/movements (مع فلترة)
        [HttpGet("movements")]
        public async Task<IActionResult> GetAllMovements(
            [FromQuery] string? type,
            [FromQuery] int? productId,
            [FromQuery] int? warehouseId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.StockMovements
                .Include(m => m.Product)
                .Include(m => m.Warehouse)
                .AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(m => m.MovementType == type);

            if (productId.HasValue)
                query = query.Where(m => m.ProductId == productId.Value);

            if (warehouseId.HasValue)
                query = query.Where(m => m.WarehouseId == warehouseId.Value);

            if (from.HasValue)
                query = query.Where(m => m.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(m => m.CreatedAt <= to.Value);

            var total = await query.CountAsync();

            var movements = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.Id,
                    m.MovementNumber,
                    m.MovementType,
                    m.Quantity,
                    ProductName = m.Product.ProductName,
                    ProductCode = m.Product.ProductCode,
                    WarehouseName = m.Warehouse.WarehouseName,
                    m.UnitPrice,
                    m.TotalPrice,
                    m.CreatedAt,
                    m.Notes
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                movements
            });
        }

        // ==================== 🔔 التنبيهات (Alerts) ====================

        // GET: api/warehouse/alerts
        [HttpGet("alerts")]
        public async Task<IActionResult> GetAlerts([FromQuery] bool onlyUnread = true)
        {
            var query = _context.StockAlerts
                .Include(a => a.Product)
                .Include(a => a.Warehouse)
                .Where(a => a.IsActive);

            if (onlyUnread)
                query = query.Where(a => !a.IsRead);

            var alerts = await query
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    a.Id,
                    a.AlertType,
                    a.Message,
                    ProductName = a.Product.ProductName,
                    ProductCode = a.Product.ProductCode,
                    WarehouseName = a.Warehouse.WarehouseName,
                    a.IsRead,
                    a.CreatedAt
                })
                .ToListAsync();

            return Ok(alerts);
        }

        // PUT: api/warehouse/alerts/{id}/read
        [HttpPut("alerts/{id}/read")]
        public async Task<IActionResult> MarkAlertAsRead(int id)
        {
            var alert = await _context.StockAlerts.FindAsync(id);
            if (alert == null)
                return NotFound(new { message = "التنبيه غير موجود" });

            alert.IsRead = true;
            alert.ReadAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تحديث التنبيه" });
        }

        // ==================== 📥 استلام بضاعة (Receive) ====================

        // POST: api/warehouse/receive
        [HttpPost("receive")]
        public async Task<IActionResult> ReceiveStock([FromBody] ReceiveStockRequest request)
        {
            if (request.Quantity <= 0 || request.UnitPrice < 0)
                return BadRequest(new { message = "الكمية يجب أن تكون موجبة والسعر غير سالب" });

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
                return NotFound(new { message = "المنتج غير موجود" });

            var warehouse = await _context.Warehouses.FindAsync(request.WarehouseId);
            if (warehouse == null)
                return NotFound(new { message = "المستودع غير موجود" });

            // تسجيل الحركة
            var movement = new StockMovement
            {
                MovementNumber = GenerateMovementNumber("IN"),
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                MovementType = "إدخال",
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice ?? product.PurchasePrice,
                TotalPrice = request.Quantity * (request.UnitPrice ?? product.PurchasePrice),
                Notes = request.Notes,
                SupplierId = request.SupplierId,
                UserId = GetCurrentUserId(),
                CreatedAt = DateTime.Now
            };

            // تحديث المخزون
            var productWarehouse = await _context.ProductWarehouses
                .FirstOrDefaultAsync(pw => pw.ProductId == request.ProductId && pw.WarehouseId == request.WarehouseId);

            if (productWarehouse == null)
            {
                productWarehouse = new ProductWarehouse
                {
                    ProductId = request.ProductId,
                    WarehouseId = request.WarehouseId,
                    Quantity = request.Quantity,
                    ReorderPoint = product.MinStock
                };
                _context.ProductWarehouses.Add(productWarehouse);
            }
            else
            {
                productWarehouse.Quantity += request.Quantity;
            }

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            // التحقق من التنبيهات بعد التحديث
            await CheckAndCreateAlerts(request.ProductId, request.WarehouseId);

            return Ok(new { message = "تم استلام البضاعة بنجاح", movement });
        }

        // ==================== 📤 إخراج بضاعة (Exit) ====================

        // POST: api/warehouse/exit
        [HttpPost("exit")]
        public async Task<IActionResult> ExitStock([FromBody] ExitStockRequest request)
        {
            if (request.Quantity <= 0 || request.UnitPrice < 0)
                return BadRequest(new { message = "الكمية يجب أن تكون موجبة والسعر غير سالب" });

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
                return NotFound(new { message = "المنتج غير موجود" });

            var productWarehouse = await _context.ProductWarehouses
                .FirstOrDefaultAsync(pw => pw.ProductId == request.ProductId && pw.WarehouseId == request.WarehouseId);

            if (productWarehouse == null || productWarehouse.Quantity < request.Quantity)
                return BadRequest(new { message = "الكمية غير متوفرة في المستودع" });

            // تسجيل الحركة
            var movement = new StockMovement
            {
                MovementNumber = GenerateMovementNumber("OUT"),
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                MovementType = "إخراج",
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice ?? product.SellingPrice,
                TotalPrice = request.Quantity * (request.UnitPrice ?? product.SellingPrice),
                Notes = request.Notes,
                CustomerId = request.CustomerId,
                UserId = GetCurrentUserId(),
                CreatedAt = DateTime.Now
            };

            // تحديث المخزون
            productWarehouse.Quantity -= request.Quantity;

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            // التحقق من التنبيهات بعد التحديث
            await CheckAndCreateAlerts(request.ProductId, request.WarehouseId);

            return Ok(new { message = "تم إخراج البضاعة بنجاح", movement });
        }

        // ==================== 🔄 نقل مخزون (Transfer) ====================

        // POST: api/warehouse/transfer
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferStock([FromBody] TransferStockRequest request)
        {
            if (request.Quantity <= 0 || request.FromWarehouseId == request.ToWarehouseId)
                return BadRequest(new { message = "بيانات النقل غير صالحة" });

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
                return NotFound(new { message = "المنتج غير موجود" });

            var fromWarehouse = await _context.ProductWarehouses
                .FirstOrDefaultAsync(pw => pw.ProductId == request.ProductId && pw.WarehouseId == request.FromWarehouseId);

            if (fromWarehouse == null || fromWarehouse.Quantity < request.Quantity)
                return BadRequest(new { message = "الكمية غير متوفرة في المستودع المصدر" });

            var toWarehouse = await _context.ProductWarehouses
                .FirstOrDefaultAsync(pw => pw.ProductId == request.ProductId && pw.WarehouseId == request.ToWarehouseId);

            // حركة إخراج من المصدر
            var exitMovement = new StockMovement
            {
                MovementNumber = GenerateMovementNumber("TRF-OUT"),
                ProductId = request.ProductId,
                WarehouseId = request.FromWarehouseId,
                MovementType = "إخراج",
                Quantity = request.Quantity,
                UnitPrice = product.PurchasePrice,
                TotalPrice = request.Quantity * product.PurchasePrice,
                Notes = $"نقل إلى مستودع {request.ToWarehouseId}",
                UserId = GetCurrentUserId(),
                CreatedAt = DateTime.Now
            };

            // حركة إدخال إلى الهدف
            var entryMovement = new StockMovement
            {
                MovementNumber = GenerateMovementNumber("TRF-IN"),
                ProductId = request.ProductId,
                WarehouseId = request.ToWarehouseId,
                MovementType = "إدخال",
                Quantity = request.Quantity,
                UnitPrice = product.PurchasePrice,
                TotalPrice = request.Quantity * product.PurchasePrice,
                Notes = $"نقل من مستودع {request.FromWarehouseId}",
                UserId = GetCurrentUserId(),
                CreatedAt = DateTime.Now
            };

            // تحديث الكميات
            fromWarehouse.Quantity -= request.Quantity;

            if (toWarehouse == null)
            {
                toWarehouse = new ProductWarehouse
                {
                    ProductId = request.ProductId,
                    WarehouseId = request.ToWarehouseId,
                    Quantity = request.Quantity,
                    ReorderPoint = product.MinStock
                };
                _context.ProductWarehouses.Add(toWarehouse);
            }
            else
            {
                toWarehouse.Quantity += request.Quantity;
            }

            // تسجيل عملية النقل
            var transfer = new StockTransfer
            {
                TransferNumber = GenerateMovementNumber("TRF"),
                ProductId = request.ProductId,
                FromWarehouseId = request.FromWarehouseId,
                ToWarehouseId = request.ToWarehouseId,
                Quantity = request.Quantity,
                Notes = request.Notes,
                UserId = GetCurrentUserId(),
                CompletedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _context.StockMovements.Add(exitMovement);
            _context.StockMovements.Add(entryMovement);
            _context.StockTransfers.Add(transfer);
            await _context.SaveChangesAsync();

            // التحقق من التنبيهات
            await CheckAndCreateAlerts(request.ProductId, request.FromWarehouseId);
            await CheckAndCreateAlerts(request.ProductId, request.ToWarehouseId);

            return Ok(new { message = "تم نقل المخزون بنجاح", transfer });
        }

        // ==================== 📋 جرد سريع (Quick Count) ====================

        // POST: api/warehouse/quick-count
        [HttpPost("quick-count")]
        public async Task<IActionResult> QuickCount([FromBody] QuickCountRequest request)
        {
            var productWarehouse = await _context.ProductWarehouses
                .FirstOrDefaultAsync(pw => pw.ProductId == request.ProductId && pw.WarehouseId == request.WarehouseId);

            if (productWarehouse == null)
                return NotFound(new { message = "المنتج غير موجود في المستودع" });

            // تسجيل حركة جرد
            var movement = new StockMovement
            {
                MovementNumber = GenerateMovementNumber("QC"),
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                MovementType = "جرد",
                Quantity = request.CountedQuantity,
                Notes = $"جرد سريع - الكمية المسجلة: {productWarehouse.Quantity}، الكمية المعدودة: {request.CountedQuantity}",
                UserId = GetCurrentUserId(),
                CreatedAt = DateTime.Now
            };

            _context.StockMovements.Add(movement);

            // تحديث الكمية إذا اختلفت
            if (productWarehouse.Quantity != request.CountedQuantity)
            {
                productWarehouse.Quantity = request.CountedQuantity;
            }

            await _context.SaveChangesAsync();

            // التحقق من التنبيهات
            await CheckAndCreateAlerts(request.ProductId, request.WarehouseId);

            return Ok(new { message = "تم تسجيل الجرد بنجاح", movement });
        }

        // ==================== 🛠️ دوال مساعدة (Helpers) ====================

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

        private async Task CheckAndCreateAlerts(int productId, int warehouseId)
        {
            var productWarehouse = await _context.ProductWarehouses
                .Include(pw => pw.Product)
                .FirstOrDefaultAsync(pw => pw.ProductId == productId && pw.WarehouseId == warehouseId);

            if (productWarehouse == null) return;

            // التحقق من وجود تنبيه مسبق لنفس المنتج والمستودع
            var existingAlert = await _context.StockAlerts
                .FirstOrDefaultAsync(a => a.ProductId == productId && a.WarehouseId == warehouseId && !a.IsRead);

            // إذا كانت الكمية 0
            if (productWarehouse.Quantity == 0)
            {
                var message = $"نفذت كمية \"{productWarehouse.Product.ProductName}\"";

                if (existingAlert == null)
                {
                    var alert = new StockAlert
                    {
                        ProductId = productId,
                        WarehouseId = warehouseId,
                        AlertType = "OutOfStock",
                        Message = message,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    _context.StockAlerts.Add(alert);
                }
                else if (existingAlert.AlertType != "OutOfStock")
                {
                    existingAlert.AlertType = "OutOfStock";
                    existingAlert.Message = message;
                }
            }
            // إذا كانت الكمية أقل من الحد الأدنى
            else if (productWarehouse.Quantity < productWarehouse.ReorderPoint)
            {
                var message = $"مخزون \"{productWarehouse.Product.ProductName}\" أقل من الحد الأدنى";

                if (existingAlert == null)
                {
                    var alert = new StockAlert
                    {
                        ProductId = productId,
                        WarehouseId = warehouseId,
                        AlertType = "LowStock",
                        Message = message,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    _context.StockAlerts.Add(alert);
                }
                else if (existingAlert.AlertType != "LowStock")
                {
                    existingAlert.AlertType = "LowStock";
                    existingAlert.Message = message;
                }
            }
            // إذا كانت الكمية طبيعية، نغلق التنبيه المفتوح
            else
            {
                if (existingAlert != null)
                {
                    existingAlert.IsRead = true;
                    existingAlert.ReadAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
        }
    }

    // ==================== 📦 DTOs (نماذج البيانات) ====================

    public class ReceiveStockRequest
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Notes { get; set; }
        public int? SupplierId { get; set; }
    }

    public class ExitStockRequest
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Notes { get; set; }
        public int? CustomerId { get; set; }
    }

    public class TransferStockRequest
    {
        public int ProductId { get; set; }
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }

    public class QuickCountRequest
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int CountedQuantity { get; set; }
    }
}