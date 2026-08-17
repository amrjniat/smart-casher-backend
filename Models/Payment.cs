namespace POS.Models
{
    public class Payment : BaseEntity
    {
        public int InvoiceId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Notes { get; set; }

        public virtual Invoice Invoice { get; set; } = null!;
        public virtual PaymentMethod PaymentMethod { get; set; } = null!;
    }
}