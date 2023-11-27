﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.ProniaAdmin.ViewModels;
using Pronia.DAL;
using Pronia.Models;

namespace Pronia.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Product> products = await _context.Products.Include(p=>p.Category).Include(p=>p.productImages.Where(pi=>pi.IsPrimary==true)).ToListAsync();
            return View (products);
        }

        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new CreateProductVM
            {
                Categories = await GetCategoriesAsync(),
            };   
            return View(productVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {

            if (!ModelState.IsValid)
            {
                productVM .Categories = await GetCategoriesAsync();
                return View(productVM);
            }

            bool result = await _context.Products.AnyAsync(p=>p.CategoryId == productVM.CategoryID);

            if (!result)
            {   productVM.Categories=await GetCategoriesAsync();
                ModelState.AddModelError("CategoryID","Bele bir kateqoriya movcud deyil");
                return View(productVM);
            }

            Product product = new Product
            {
                Name = productVM.Name,
                SKU = productVM.SKU,
                Price = productVM.Price,
                CategoryId = productVM.CategoryID,
                Description = productVM.Description
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            Product existed = await _context.Products.FirstOrDefaultAsync(p=>p.Id == id);
            if (existed == null) return NotFound();
            _context.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //public async Task<IActionResult> Update(int id)
        //{
        //    if(id <= 0) return BadRequest();
        //    Product product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

        //    if (product == null) return NotFound();

        //    UpdateProductVM productVM = new UpdateProductVM
        //    {
        //        Name = product.Name,
        //        Price = product.Price,
        //        Description = product.Description,
        //        SKU = product.SKU,
        //        CategoryId = product.CategoryId,
        //        Categories = await GetCategoriesAsync()
        //    };
        //    return View(productVM);
        //}


        //[HttpPost]

        //public async Task<IActionResult> Update (int id, UpdateProductVM productVM)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        Product findedProduct = await _context.Products.FirstOrDefaultAsync(p=>p.Id == id);

        //        productVM.Categories = await GetCategoriesAsync();
        //        return View(productVM);
        //    }

        //    Product product = await _context.Products.Include(p=>p.CategoryId == id).FirstOrDefaultAsync(p => p.Id == id);

        //    bool result = await _context.Products.AnyAsync(c => c.Id == productVM.CategoryId);
        //    if (!result)
        //    {
        //        ModelState.AddModelError("CategoryId", "bele bir kateqoriya yoxdur");
        //        productVM.Categories = await GetCategoriesAsync();
        //        return View(productVM);
        //    }


        //}






        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.Include(p => p.productImages.Where(pi => pi.IsPrimary == true)).Include(p => p.Category).Include(p => p.ProductTags).ThenInclude(pt => pt.Tag).Include(p => p.ProductColors).ThenInclude(pc => pc.Color).Include(p => p.ProductSizes).ThenInclude(ps => ps.Size).FirstOrDefaultAsync(p => p.Id == id);
            if(product == null) return NotFound();
            return View(product);
        }


        private async Task<List<Category>> GetCategoriesAsync()
        {
            List<Category> categories = await _context.Categories.ToListAsync();
            return categories;
        }






    }


    

}
