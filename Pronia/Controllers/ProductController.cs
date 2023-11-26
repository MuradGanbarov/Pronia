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

        public async Task<IActionResult> Detail(int id)
        {
            if(id <=0)
            {
                return BadRequest();
            }

            Product product = await _context.Products.
            Include(p => p.productImages).
            Include(p => p.Category).
            Include(p=>p.ProductColors).
            ThenInclude(pc=>pc.Color).
            Include(p=>p.ProductSizes).
            ThenInclude(ps=>ps.Size).
            Include(p => p.ProductTags).
            ThenInclude(pt=>pt.Tag).
            FirstOrDefaultAsync(p => p.Id == id);
            
            if(product == null)
            {
                return NotFound();
            }

            List<Product> SimilarProducts = await _context.Products.Include(product => product.productImages.Where(pi=>pi.IsPrimary!=null)).Where(p => p.CategoryId == product.CategoryId && product.Id != p.Id).Take(4).ToListAsync();

            ProductVMcs productVMcs = new ProductVMcs
            {
                Product= product,
                SimilarProducts= SimilarProducts
            };


            return View(productVMcs);
        }


    }
}
