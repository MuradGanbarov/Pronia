using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;

namespace Pronia.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class TagsController : Controller
    {
        private readonly AppDbContext _context;
        public TagsController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {


            List<Tag> tags = await _context.Tags.Include(t => t.ProductTags).ThenInclude(pt=>pt.Product.Name).ToListAsync();
            return View(tags);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Tag tags)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = _context.Tags.Any(t => t.Name.ToLower().Trim() == t.Name.ToLower().Trim());

            if (result)
            {
                ModelState.AddModelError("Name", "Bele bir category artiq movcuddur");
                return View();
            }

            await _context.Tags.AddAsync(tags);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");

        }
    }
}

