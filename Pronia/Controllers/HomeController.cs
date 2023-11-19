using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModel;

namespace Pronia.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            List<Slide> slides=_context.Slides.OrderBy(s=>s.Order).Take(2).ToList();
            List<Product> products = _context.Products.Include(p=>p.productImages).OrderByDescending(s => s.Id).ToList();


            HomeVMcs vm = new()
            {
                Slides = slides,
                Products = products,
                NewProduct = products.Take(8).ToList()
            };

            return View(vm);
        }
        
    }
}
