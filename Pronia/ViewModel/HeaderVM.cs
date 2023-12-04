using Pronia.Models;

namespace Pronia.ViewModel
{
    public class HeaderVM
    {
        public List<BasketItemVM> BasketItems { get; set; }
        public Dictionary<string,string> Settings { get; set; }
    }
}
