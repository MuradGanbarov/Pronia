using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.ProniaAdmin.Models;
using Pronia.Areas.ProniaAdmin.Models;
using Pronia.Areas.ProniaAdmin.Models;
using Pronia.Areas.ProniaAdmin.Models.Utilities.Enums;
using Pronia.Areas.ViewModels.Slide;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilites.Enums;
using static Pronia.Areas.ProniaAdmin.Models.Utilities.Extensions.AuthorizeRoles;

namespace Pronia.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
    public class SlideController : Controller
    {
        
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public SlideController(AppDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Slide> slides = await _context.Slides.ToListAsync();
            return View(slides);
        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSlideVM slideVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //if (slideVM.Photo is null)
            //{
            //    ModelState.AddModelError("Photo", "Mutleq shekil sechilmelidir");
            //    return View();
            //}
            if (!slideVM.Photo.IsValidType(FileType.Image))
            {
                ModelState.AddModelError("Photo", "File'in type uygun deyil");
                return View();
            }
            if (!slideVM.Photo.IsValidSize(2, FileSize.Megabite))
            {
                ModelState.AddModelError("Photo", "Sheklin hecmi 2 mb-den olmamalidir");
                return View();
            }

            if (slideVM.Order <= 0)
            {
                ModelState.AddModelError("Order", "Order 0 dan kichik yada 0 beraber olmali deyil");
            }

            string fileName = await slideVM.Photo.CreateAsync(_env.WebRootPath, "assets", "images", "slider");

            Slide slide = new Slide 
            {
                ImageURL = fileName,
                Title = slideVM.Title,
                SubTitle = slideVM.SubTitle,
                Description = slideVM.Description,
                Order = (int)slideVM.Order
            };
            
            

            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [AuthorizeRolesAttribute(UserRole.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (slide is null) return NotFound();
            slide.ImageURL.Delete(_env.WebRootPath,"assets","images","slider");
            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public async Task<IActionResult> Update(int id)
        {
            if(id<=0) return BadRequest();
            Slide existed = await _context.Slides.FirstOrDefaultAsync(s=>s.Id == id);
            if (existed is null) return NotFound();

            return View(existed);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, Slide slide)
        {
            Slide existed = _context.Slides.FirstOrDefault(s=>s.Id == id);
            if (existed is null) return NotFound();
            if (!ModelState.IsValid)
            {
                return View(slide);
            }
            
            if (slide.Photo is not null)
            {
                
                if (!slide.Photo.IsValidType(FileType.Image))
                {
                    ModelState.AddModelError("Photo", "File'in type uygun deyil");
                    return View();
                }
                if (!slide.Photo.IsValidSize(2, FileSize.Megabite))
                {
                    ModelState.AddModelError("Photo", "Sheklin hecmi 2 mb-den olmamalidir");
                    return View();
                }
                string NewImage = await slide.Photo.CreateAsync(_env.WebRootPath, "assets", "images", "slider");
                existed.ImageURL.Delete(_env.WebRootPath, "assets", "images", "slider");
                existed.ImageURL = NewImage;

            }

            existed.Title = slide.Title;
            existed.Description = slide.Description;
            existed.SubTitle = slide.SubTitle;
            existed.Order = slide.Order;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


            

            
        }


        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();
            Slide slide = await _context.Slides.FirstOrDefaultAsync(s=>s.Id == id);
            if(slide is null) return NotFound();
            return View(slide);
            

        }
    }
}
