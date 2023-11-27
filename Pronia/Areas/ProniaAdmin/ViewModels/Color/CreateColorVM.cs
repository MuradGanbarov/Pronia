using Pronia.Models;

namespace Pronia.Areas.ProniaAdmin.ViewModels
{
    public class CreateColorVM
    {
        public string? Name { get; set; }
        public List<ProductColor>? ProductColors { get; set; }
    }
}
