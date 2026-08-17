namespace POS.DTOs
{
    // الطلب: تعديل رصيد منتج (إضافة / سحب / جرد)
    public class StockAdjustDto
    {
        public string Type { get; set; } = string.Empty; // "add" | "subtract" | "set"
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public int WarehouseId { get; set; } = 1;
    }

    // الرد: عنصر سجل حركة واحد (لعرضه بالـ History Drawer بالفرونت)
    public class StockMovementResponseDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // "add" | "subtract" | "set"
        public int Quantity { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}