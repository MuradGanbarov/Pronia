﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.ViewModels.Category;
using Pronia.DAL;
using Pronia.Models;


namespace Pronia.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
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
        
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Category? category = await _context.Categories.FirstOrDefaultAsync(c=>c.Id == id);
            
            if (category is null) return NotFound();

            return View(category);


        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }

            Category? existed = await _context.Categories.FirstOrDefaultAsync(c=>c.Id==id);
            if(existed is null) return NotFound();
            
            bool result = _context.Categories.Any(c => c.Name.ToLower().Trim() == category.Name.ToLower().Trim()&&c.Id!=id);
            if (result)
            {
                ModelState.AddModelError("Name","Bele bir kateqoriya hal hazirda var");
                return View();
            }

            existed.Name = category.Name;
            _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            
            
        }

        public async Task<IActionResult> Delete(int id)
        {
            if(id <= 0) return BadRequest();

            Category existed = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if( existed is null) return NotFound();

            _context.Categories.Remove(existed);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            List<Product>? products = await _context.Products.Include(p=>p.productImages).Where(p => p.CategoryId == id).ToListAsync();
            return View(products);
        }
        

    }
}
