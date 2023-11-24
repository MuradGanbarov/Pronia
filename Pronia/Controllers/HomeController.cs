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
        public async Task<IActionResult> Index()
        {
            List<Slide> slides=await _context.Slides.OrderBy(s=>s.Order).Take(2).ToListAsync();
            List<Product> products = await _context.Products.Include(p=>p.productImages.Where(pi=>pi.IsPrimary!=null)).OrderByDescending(s => s.Id).ToListAsync();

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
