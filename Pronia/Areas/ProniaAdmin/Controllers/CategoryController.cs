using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.ProniaAdmin.ViewModels;
using Pronia.Areas.ViewModels;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilites.Enums;
using Pronia.Utilites.Exceptions;
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
        public async Task<IActionResult> Index(int page)
        {
            double count = await _context.Categories.CountAsync();

            List<Category>? categories = await _context.Categories.Skip(page*3).Take(3).Include(c => c.Products).ToListAsync();

            PaginationVM<Category> paginationVM = new PaginationVM<Category>()
            {
                CurrentPage = page + 1,
                TotalPage = Math.Ceiling(count / 3),
                Items = categories,
            };
            
            return View(paginationVM);



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
            return View(category);


        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateCategoryVM categoryVM)
        {
            if(!ModelState.IsValid)
            {
                return View(categoryVM);
            }

            Category existed = await _context.Categories.FirstOrDefaultAsync(c=>c.Id==id);
            if(existed is null) return NotFound();
            
            bool ExistCheck = _context.Categories.Any(c => c.Name.ToLower().Trim() == existed.Name.ToLower().Trim()&&c.Id!=id);
            if (ExistCheck)
            {
                ModelState.AddModelError("Name","Bele bir kateqoriya hal hazirda var");
                return View(categoryVM);
            };

            
            existed.Name = categoryVM.Name;
            _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));     
        }
        [AuthorizeRolesAttribute(UserRole.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            if(id <= 0) return BadRequest();

            Category existed = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if(existed is null) return NotFound();

            _context.Categories.Remove(existed);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) return BadRequest();
            Category category = await _context.Categories.Include(c => c.Products).ThenInclude(p=>p.productImages.Where(pi=>pi.IsPrimary==true)).FirstOrDefaultAsync(c => c.Id == id);
            if (category is null) return NotFound();
            return View(category);
        }
        

    }
}
