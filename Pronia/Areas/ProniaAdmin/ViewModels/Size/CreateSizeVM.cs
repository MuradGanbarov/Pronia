using System.ComponentModel.DataAnnotations;

namespace Pronia.Areas.ProniaAdmin.ViewModels.Size
{
    public class CreateSizeVM
    {
        [Required(ErrorMessage = "Title mutleq daxil edilmelidir")]
        [MaxLength(25, ErrorMessage = "Uzunluqu 25 xarakterden chox olmamalidir")]
        public string Name { get; set; }
    }
}
