using Pronia.Models;
using System.ComponentModel.DataAnnotations;

namespace Pronia.Areas.ProniaAdmin.ViewModels.Product
{
    public class UpdateProductVMcs
    {
        public class UpdateProductVM
        {
            public string Name { get; set; }

            [Range(0, int.MaxValue, ErrorMessage = "Price must be bigger than 0")]
            [Required]
            public double? Price { get; set; }

            [Range(1, 300, ErrorMessage = "Chooce a category!")]
            public int CategoryId { get; set; }
            public string Description { get; set; }
            public string SKU { get; set; }

            public List<Category>? Categories { get; set; }

        }
    }
}
