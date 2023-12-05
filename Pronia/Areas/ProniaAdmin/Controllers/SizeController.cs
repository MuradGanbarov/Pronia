using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.ProniaAdmin.ViewModels;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilites.Enums;
using System.Drawing;
using static Pronia.Areas.ProniaAdmin.Models.Utilities.Extensions.AuthorizeRoles;
using Size = Pronia.Models.Size;

namespace Pronia.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
    public class SizeController : Controller
    {
        private readonly AppDbContext _context;
        public SizeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Size> sizes = await _context.Sizes.Include(s=>s.ProductSizes).ToListAsync();
            return View(sizes);
        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSizeVM sizeVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = _context.Sizes.Any(c => c.Name.ToLower().Trim() == sizeVM.Name.ToLower().Trim());

            if (result)
            {
                ModelState.AddModelError("Name", "Bele bir tag artiq movcuddur");
                return View();
            }

            Size size = new Size
            {
                Name = sizeVM.Name,
            };


            await _context.Sizes.AddAsync(size);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Size size = await _context.Sizes.FirstOrDefaultAsync(c => c.Id == id);

            if (size is null) return NotFound();

            return View(size);


        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, Size size)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            Size existed = await _context.Sizes.FirstOrDefaultAsync(c => c.Id == id);
            if (existed is null) return NotFound();

            bool result = _context.Sizes.Any(s => s.Name.ToLower().Trim() == size.Name.ToLower().Trim() && s.Id != id);
            if (result)
            {
                ModelState.AddModelError("Name", "Bele bir tag hal hazirda var");
                return View();
            }

            existed.Name = size.Name;
            _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


        }
        [AuthorizeRolesAttribute(UserRole.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            Size existed = await _context.Sizes.FirstOrDefaultAsync(c => c.Id == id);

            if (existed is null) return NotFound();

            _context.Sizes.Remove(existed);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public async Task<IActionResult> Details(int id)
        {

            if (id <= 0) return BadRequest();

            Size size = await _context.Sizes.Include(s => s.ProductSizes).ThenInclude(ps => ps.Product).ThenInclude(p => p.productImages).FirstOrDefaultAsync(s => s.Id == id);


            if (size is null) return NotFound();


            return View(size);


        }
    }

}

