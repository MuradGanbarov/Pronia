using System.ComponentModel.DataAnnotations;

namespace Pronia.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Ad mutleq daxil edilmelidir")]
        [MaxLength(25,ErrorMessage = "Uzunluqu 25 xarakterden chox olmamalidir")]
        public string? Name { get; set; }
        public List<Product>? Products { get; set; }
    }
}
