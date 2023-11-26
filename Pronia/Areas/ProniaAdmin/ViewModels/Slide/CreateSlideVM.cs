using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pronia.Areas.ViewModels.Slide
{
    public class CreateSlideVM
    {
        [Required(ErrorMessage = "Title mutleq daxil edilmelidir")]
        [MaxLength(25, ErrorMessage = "Uzunluqu 25 xarakterden chox olmamalidir")]
        public string? Title { get; set; }
        [Required(ErrorMessage = "Title mutleq daxil edilmelidir")]
        [MaxLength(25, ErrorMessage = "Uzunluqu 25 xarakterden chox olmamalidir")]
        public string? SubTitle { get; set; }
        [Required(ErrorMessage = "Subtitle mutleq daxil edilmelidir")]
        [MaxLength(25, ErrorMessage = "Uzunluqu 25 xarakterden chox olmamalidir")]
        public string? Description { get; set; }
        [Required]
        public int? Order { get; set; }
        [Required]
        public IFormFile? Photo { get; set; }
    }
}
