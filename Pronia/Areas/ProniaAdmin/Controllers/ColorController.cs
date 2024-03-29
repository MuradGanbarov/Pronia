﻿using Microsoft.AspNetCore.Mvc;
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
    public class ColorController : Controller
    {
        private readonly AppDbContext _context;
        public ColorController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Color> colors = await _context.Colors.Include(c => c.ProductColors).ThenInclude(pc=>pc.Product).ToListAsync();
            return View(colors);
        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateColorVM colorVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = _context.Colors.Any(c => c.Name.ToLower().Trim() == colorVM.Name.ToLower().Trim());

            if (result)
            {
                ModelState.AddModelError("Name", "Bele bir tag artiq movcuddur");
                return View();
            }

            Color color = new Color
            {
                Name = colorVM.Name,
            };


            await _context.Colors.AddAsync(color);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Color color = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);

            if (color is null) return NotFound();

            return View(color);


        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, Color color)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            Color existed = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
            if (existed is null) return NotFound();

            bool result = _context.Colors.Any(c => c.Name.ToLower().Trim() == color.Name.ToLower().Trim() && c.Id != id);
            if (result)
            {
                ModelState.AddModelError("Name", "Bele bir tag hal hazirda var");
                return View();
            }

            existed.Name = color.Name;
            _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


        }
        [AuthorizeRolesAttribute(UserRole.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            Color existed = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);

            if (existed is null) return NotFound();

            _context.Colors.Remove(existed);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
        public async Task<IActionResult> Details(int id)
        {

            if (id <= 0) return BadRequest();

            Color color = await _context.Colors.Include(c => c.ProductColors).ThenInclude(pc => pc.Product).ThenInclude(p => p.productImages).FirstOrDefaultAsync(t => t.Id == id);


            if (color is null) return NotFound();


            return View(color);


        }
    }




}

