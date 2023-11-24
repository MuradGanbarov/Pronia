using System.ComponentModel.DataAnnotations;

namespace Pronia.Models
{
    public class Size
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Title mutleq daxil edilmelidir")]
        [MaxLength(25, ErrorMessage = "Uzunluqu 25 xarakterden chox olmamalidir")]
        public string Name { get; set; }
        public List<ProductSize>? ProductSizes { get; set; }
    }
}
