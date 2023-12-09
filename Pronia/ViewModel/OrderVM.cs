using Pronia.Models;

namespace Pronia.ViewModel
{
    public class OrderVM
    {
        public string Address { get; set; }
        public List<BasketItem>? BasketItems { get; set; }
    }
}
