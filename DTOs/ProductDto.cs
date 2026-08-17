// namespace POS.DTOs
// {
//     public class ProductDto
//     {
//         public string ProductName { get; set; } = string.Empty;
//         public string ProductCode { get; set; } = string.Empty;
//         public string? Barcode { get; set; }
//         public decimal PurchasePrice { get; set; }
//         public decimal SellingPrice { get; set; }
//         public decimal TaxRate { get; set; }
//         public int MinStock { get; set; }
//         public string? Description { get; set; }
//         public int? CategoryId { get; set; }
//         public int UnitId { get; set; }
//         public int? SupplierId { get; set; }
//     }
// }




namespace POS.DTOs
{
    public class ProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal TaxRate { get; set; }
        public int MinStock { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public int UnitId { get; set; }
        public int? SupplierId { get; set; }

        // ✅ جديد: الكمية الافتتاحية القادمة من AddProductModal.tsx
        // تُستخدم فقط عند الإضافة (Create) لإنشاء صف ProductWarehouse مرتبط
        public int InitialQuantity { get; set; } = 0;
    }
}