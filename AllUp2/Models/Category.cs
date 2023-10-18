using System.ComponentModel.DataAnnotations.Schema;

namespace AllUp2.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Image { get; set; }
        public bool IsDeactive { get; set; }
        public bool IsMain { get; set; }
        public List<Category>? Children { get; set; }
        public List<ProductCategory> ProductCategories { get; set; }
        public Category? Parent { get; set; }
        public int? ParentId { get; set; }
        [NotMapped]
        public IFormFile? Photo { get; set; }
    }
}
