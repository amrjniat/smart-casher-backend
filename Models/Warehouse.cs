namespace POS.Models
{
    public class Warehouse : BaseEntity
    {
        public string WarehouseName { get; set; } = string.Empty;
        public string? WarehouseCode { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? ManagerName { get; set; }
        public bool IsMainWarehouse { get; set; } = false;

        public int? BranchId { get; set; }

        public virtual Branch? Branch { get; set; }
        public virtual ICollection<ProductWarehouse> ProductWarehouses { get; set; } = new List<ProductWarehouse>();
    }
}