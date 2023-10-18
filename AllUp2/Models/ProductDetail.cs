using System.ComponentModel.DataAnnotations.Schema;

namespace AllUp2.Models
{
    public class ProductDetail
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string ProductCode { get; set; }
        public float Tax { get; set; }
        public bool hasStock { get; set; }
        public Product Product { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }
    }
}
