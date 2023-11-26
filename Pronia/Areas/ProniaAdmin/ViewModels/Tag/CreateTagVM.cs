using System.ComponentModel.DataAnnotations;

namespace Pronia.Areas.ProniaAdmin.ViewModels.Tag
{
    public class CreateTagVM
    {

        [Required(ErrorMessage = "Ad mutleq daxil edilmelidir")]
        [MaxLength(25, ErrorMessage = "Uzunluqu 25 xarakterden chox olmamalidir")]
        public string Name { get; set; }

    }
}
