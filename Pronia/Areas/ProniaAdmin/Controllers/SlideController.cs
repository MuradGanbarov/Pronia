using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.ContentModel;
using Pronia.Areas.ProniaAdmin.Models.Utilities.Enums;
using Pronia.Areas.ProniaAdmin.Models.Utilities.Extensions;
using Pronia.DAL;
using Pronia.Models;
using System.Collections.Generic;

namespace Pronia.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
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
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Slide slide)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }

            if (slide.Photo is null)
            {
                ModelState.AddModelError("Photo", "Mutleq shekil sechilmelidir");
                return View();
            }
            if (!slide.Photo.IsValidType(FileType.Image))
            {
                ModelState.AddModelError("Photo", "File'in type uygun deyil");
                return View();            
            }
            if (!slide.Photo.IsValidSize(2,FileSize.Megabite))
            {
                ModelState.AddModelError("Photo", "Sheklin hecmi 2 mb-den olmamalidir");
                return View();
            }
            
            if (slide.Order <= 0)
            {
                ModelState.AddModelError("Order", "Order 0 dan kichik yada 0 beraber olmali deyil");
            }

            slide.ImageURL = await slide.Photo.CreateAsync(_env.WebRootPath,"assets","images","slider");

            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

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


        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();
            Slide slide = await _context.Slides.FirstOrDefaultAsync(s=>s.Id == id);
            if(slide is null) return NotFound();
            return View(slide);
            

        }
    }
}
