using System.ComponentModel.DataAnnotations.Schema;

namespace AllUp2.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
        public float? OldPrice { get; set; }
        public bool IsDeactive { get; set; }
        public int Rate { get; set; }
        public Brand Brand { get; set; }
        public int BrandId { get; set; }
        [NotMapped]
        public IFormFile[] Photos { get; set; }
        public List<ProductCategory> ProductCategories { get; set; }
        public List<ProductTag> ProductTags { get; set; }
        public List<ProductImage> ProductImages { get; set; }
        public ProductDetail ProductDetail { get; set; }

    }
}
