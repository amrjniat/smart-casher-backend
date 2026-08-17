namespace POS.Models
{
    public class StockMovement : BaseEntity
    {
        public string MovementNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public string MovementType { get; set; } = string.Empty; // "إدخال", "إخراج", "جرد"
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Notes { get; set; }
        public int? SupplierId { get; set; }
        public int? CustomerId { get; set; }
        public int? UserId { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual Supplier? Supplier { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual User? User { get; set; }
    }
}