using Microsoft.EntityFrameworkCore;
using Pronia.Models;

namespace Pronia.ViewModel
{
    public class HomeVMcs
    {
        public List<Slide> Slides { get; set; }
        public List<Product> Products { get; set; }
        
    }
}
