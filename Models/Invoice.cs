namespace POS.Models
{
    public class Invoice : BaseEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int BranchId { get; set; }
        public int? UserId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty; // "مدفوعة", "غير مدفوعة", "ملغاة"
        public string? Notes { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual User? User { get; set; }
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}