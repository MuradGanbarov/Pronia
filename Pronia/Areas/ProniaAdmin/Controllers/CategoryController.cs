using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.ProniaAdmin.ViewModels;
using Pronia.Areas.ViewModels;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilites.Enums;
using static Pronia.Areas.ProniaAdmin.Models.Utilities.Extensions.AuthorizeRoles;

namespace Pronia.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    [AuthorizeRolesAttribute(UserRole.Admin,UserRole.Moderator)]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Category>? categories = await _context.Categories.Include(c => c.Products).ToListAsync();
            return View(categories);
        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryVM categoryVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = _context.Categories.Any(c => c.Name.ToLower().Trim() == categoryVM.Name.ToLower().Trim());

            if (result)
            {
                ModelState.AddModelError("Name","Bele bir category artiq movcuddur");
                return View();           
            }

            Category category = new Category
            {
                Name = categoryVM.Name,
            };


            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Category category = await _context.Categories.FirstOrDefaultAsync(c=>c.Id == id);
            
            if (category is null) return NotFound();
            UpdateCategoryVM categoryVM = new UpdateCategoryVM { Name = category.Name };
            return View(categoryVM);


        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateCategoryVM categoryVM)
        {
            if(!ModelState.IsValid)
            {
                return View(categoryVM);
            }

            Category category = await _context.Categories.FirstOrDefaultAsync(c=>c.Id==id);
            if(category is null) return NotFound();
            
            bool ExistCheck = _context.Categories.Any(c => c.Name.ToLower().Trim() == category.Name.ToLower().Trim()&&c.Id!=id);
            if (ExistCheck)
            {
                ModelState.AddModelError("Name","Bele bir kateqoriya hal hazirda var");
                return View(categoryVM);
            };

            if (category.Name == categoryVM.Name) return RedirectToAction(nameof(Index));
            category.Name = categoryVM.Name;
            _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));     
        }
        [AuthorizeRolesAttribute(UserRole.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            if(id <= 0) return BadRequest();

            Category existed = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if( existed is null) return NotFound();

            _context.Categories.Remove(existed);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public async Task<IActionResult> Details(int id)
        {
            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            List<Product>? products = await _context.Products.Include(p=>p.productImages).Where(p => p.CategoryId == id).ToListAsync();
            return View(products);
        }
        

    }
}
