namespace POS.DTOs
{
    public class StockMovementDto
    {
        public int Id { get; set; }
        public string MovementNumber { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UserName { get; set; }
        public string? SupplierName { get; set; }
        public string? CustomerName { get; set; }
    }

    public class AdjustStockRequestDto
    {
        public int? NewQuantity { get; set; }
        public int? Quantity { get; set; }
        public int WarehouseId { get; set; } = 1;
        public string Type { get; set; } = "add";
        public string? Notes { get; set; }
        public string? Reason { get; set; }
    }

    public class AdjustStockResponseDto
    {
        public int ProductId { get; set; }
        public int PreviousQuantity { get; set; }
        public int NewQuantity { get; set; }
        public int Difference { get; set; }
        public StockMovementDto Movement { get; set; } = null!;
    }
}