using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModel;

namespace Pronia.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        public ProductController(AppDbContext context)
        {
            _context = context;
        }


        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Detail(int id)
        {
            if(id <=0)
            {
                return BadRequest();
            }

            Product product = _context.Products.Include(p=>p.productImages).Include(p=>p.Category).FirstOrDefault(p => p.Id == id);
            
            if(product == null)
            {
                return NotFound();
            }

            List<Product> SimilarProducts = _context.Products.Include(product => product.productImages).Where(p => p.CategoryId == product.CategoryId && product.Id != p.Id).Take(4).ToList();

            ProductVMcs productVMcs = new ProductVMcs
            {
                Product= product,
                SimilarProducts= SimilarProducts
            };


            return View(productVMcs);
        }


    }
}
