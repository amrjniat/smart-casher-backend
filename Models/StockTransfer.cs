namespace POS.Models
{
    public class StockTransfer : BaseEntity
    {
        public string TransferNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public int? UserId { get; set; }
        public DateTime? CompletedAt { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual Warehouse FromWarehouse { get; set; } = null!;
        public virtual Warehouse ToWarehouse { get; set; } = null!;
        public virtual User? User { get; set; }
    }
}