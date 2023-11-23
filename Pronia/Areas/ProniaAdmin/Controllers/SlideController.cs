﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using System.Collections.Generic;

namespace Pronia.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class SlideController : Controller
    {
        
        private readonly AppDbContext _context;
        public SlideController(AppDbContext context)
        {
            _context = context;
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
            if (slide.Photo is null)
            {
                ModelState.AddModelError("Photo", "Mutleq shekil sechilmelidir");
                return View();
            }
            if (slide.Photo.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Photo", "File tipi uygun deyil");
                return View();
            }
            if (slide.Photo.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("Photo", "Sheklin hecmi 2 mb-den olmamalidir");
            }
            if (!ModelState.IsValid)
            {
                return View();
            }
            if(slide.Order <= 0)
            {
                ModelState.AddModelError("Order","Order 0 dan kichik yada 0 beraber olmali deyil");
            }

            FileStream file = new FileStream(@"C:\Users\Murad\Desktop\Pronia\Pronia\wwwroot\assets\images\slider\" + slide.Photo.FileName, FileMode.Create);

            await slide.Photo.CopyToAsync(file);
            file.Close();
            slide.ImageURL = slide.Photo.FileName;

            await _context.Slides.AddAsync(slide);
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
