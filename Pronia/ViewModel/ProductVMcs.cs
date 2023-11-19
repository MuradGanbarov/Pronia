using Pronia.Models;

namespace Pronia.ViewModel
{
    public class ProductVMcs
    {
        public Product Product { get; set; }
        public List<Product>? SimilarProducts { get; set; }
    }
}
