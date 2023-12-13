using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.ProniaAdmin.ViewModels;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilites.Enums;
using static Pronia.Areas.ProniaAdmin.Models.Utilities.Extensions.AuthorizeRoles;

namespace Pronia.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
    public class TagsController : Controller
    {
        private readonly AppDbContext _context;
        public TagsController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page)
        {
            double count = await _context.Tags.CountAsync();
            List<Tag> tags = await _context.Tags.Skip(page*3).Take(3).Include(t => t.ProductTags).ToListAsync();

            PaginationVM<Tag> paginationVM = new PaginationVM<Tag>
            {
                CurrentPage = page + 1,
                TotalPage = Math.Ceiling(count/3),
                Items = tags
            };

            return View(paginationVM);
        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTagVM tagVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = _context.Tags.Any(t => t.Name.ToLower().Trim() == tagVM.Name.ToLower().Trim());

            if (result)
            {
                ModelState.AddModelError("Name","Bele bir tag artiq movcuddur");
                return View();
            }

            Tag tag = new Tag
            {
                Name = tagVM.Name,
            };

            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Tag tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);

            if (tag is null) return NotFound();

            return View(tag);


        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            Tag existed = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (existed is null) return NotFound();

            bool result = _context.Categories.Any(t => t.Name.ToLower().Trim() == tag.Name.ToLower().Trim() && t.Id != id);
            if (result)
            {
                ModelState.AddModelError("Name", "Bele bir tag hal hazirda var");
                return View();
            }

            existed.Name = tag.Name;
            _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


        }
        [AuthorizeRolesAttribute(UserRole.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            Tag existed = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);

            if (existed is null) return NotFound();

            _context.Tags.Remove(existed);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public async Task<IActionResult> Details(int id)
        {

            if (id <= 0) return BadRequest();

            Tag tag =  await _context.Tags.Include(t=>t.ProductTags).ThenInclude(pt=>pt.Product).ThenInclude(p=>p.productImages).FirstOrDefaultAsync(t => t.Id == id);


            if (tag is null) return NotFound();


            return View(tag);


        }
    }
}

