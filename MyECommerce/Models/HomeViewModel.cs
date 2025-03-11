namespace MyECommerce.Models
{
    public class HomeViewModel
    {
        public List<Product> HeroProducts { get; set; } = new();
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();
        public List<Product> AllProducts { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<GalleryImage> GalleryImages { get; set; } = new();
    }
}
