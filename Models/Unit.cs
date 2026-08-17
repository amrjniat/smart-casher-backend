namespace POS.Models
{
    public class Unit : BaseEntity
    {
        public string UnitName { get; set; } = string.Empty;
        public string? UnitSymbol { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}