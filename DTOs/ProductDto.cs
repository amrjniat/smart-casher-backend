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
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string ProductName { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(50)]
        public string ProductCode { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        [System.ComponentModel.DataAnnotations.Range(0, double.MaxValue)]
        public decimal PurchasePrice { get; set; }
        [System.ComponentModel.DataAnnotations.Range(0, double.MaxValue)]
        public decimal SellingPrice { get; set; }
        [System.ComponentModel.DataAnnotations.Range(0, 100)]
        public decimal TaxRate { get; set; }
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
        public int MinStock { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
        public int UnitId { get; set; }
        public int? SupplierId { get; set; }

        // ✅ جديد: الكمية الافتتاحية القادمة من AddProductModal.tsx
        // تُستخدم فقط عند الإضافة (Create) لإنشاء صف ProductWarehouse مرتبط
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
        public int InitialQuantity { get; set; } = 0;
    }
}