using System.ComponentModel.DataAnnotations;

namespace Pronia.Models
{
    public class Tag
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Ad mutleq daxil edilmelidir")]
        [MaxLength(25, ErrorMessage = "Uzunluqu 25 xarakterden chox olmamalidir")]
        public string Name { get; set; }

        public List<ProductTag> ProductTags { get; set; }
    }
}
