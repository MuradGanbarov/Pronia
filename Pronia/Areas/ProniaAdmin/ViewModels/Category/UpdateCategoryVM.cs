using System.ComponentModel.DataAnnotations;

namespace Pronia.Areas.ProniaAdmin.ViewModels;

public class UpdateCategoryVM
{
    [MaxLength(ErrorMessage = "Maksimum ad 30 olmalidir!")]
    public string? Name { get; set; }

}
