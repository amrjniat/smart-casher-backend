namespace POS.Models
{
    public class StockAlert : BaseEntity
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public string AlertType { get; set; } = string.Empty; // "LowStock", "OutOfStock"
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
    }
}