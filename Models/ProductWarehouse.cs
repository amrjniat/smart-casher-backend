namespace POS.Models
{
    public class ProductWarehouse : BaseEntity
    {
        public int Quantity { get; set; } = 0;
        public int ReservedQuantity { get; set; } = 0;
        public int AvailableQuantity => Quantity - ReservedQuantity;
        public int ReorderPoint { get; set; } = 0;

        public int ProductId { get; set; }
        public int WarehouseId { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
    }
}