using Pronia.Models;
using System.ComponentModel.DataAnnotations;

namespace Pronia.Areas.ProniaAdmin.ViewModels.Product
{
    public class CreateProductVM
    {
        public string Name { get; set; }

        [Range(1,int.MaxValue, ErrorMessage = "Qiymeti 0 ya 0dan ashaqi ola bilmez")]
        public double Price { get; set; }
        [Required]
        [MinLength(10,ErrorMessage = "Description hissesine azi 10 herif olmalidir.")]
        public string Description { get; set; }
        [Required]
        public string SKU { get; set; }
        [Range(1, 300, ErrorMessage = "Chooce a category!")]

        public int CategoryID { get; set; }
        public List<Category>? Categories { get; set; }

    }
}
