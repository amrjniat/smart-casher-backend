namespace POS.Models
{
    public class Category : BaseEntity
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryCode { get; set; }
        public string? Description { get; set; }
        public string? IconPath { get; set; }
        public int DisplayOrder { get; set; } = 0;

        public int? ParentCategoryId { get; set; }

        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Category> ChildCategories { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}