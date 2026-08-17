namespace POS.Models
{
    public class Product : BaseEntity
    {
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? SKU { get; set; }
        public decimal PurchasePrice { get; set; } = 0;
        public decimal SellingPrice { get; set; } = 0;
        public decimal? WholesalePrice { get; set; }
        public decimal TaxRate { get; set; } = 0;
        public int MinStock { get; set; } = 0;
        public int MaxStock { get; set; } = 0;
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public decimal? Weight { get; set; }
        public bool IsTaxable { get; set; } = true;
        public bool IsTrackStock { get; set; } = true;
        public bool IsService { get; set; } = false;

        public int? CategoryId { get; set; }
        public int UnitId { get; set; }
        public int? SupplierId { get; set; }

        public virtual Category? Category { get; set; }
        public virtual Unit Unit { get; set; } = null!;
        public virtual Supplier? Supplier { get; set; }
        public virtual ICollection<ProductWarehouse> ProductWarehouses { get; set; } = new List<ProductWarehouse>();
    }
}