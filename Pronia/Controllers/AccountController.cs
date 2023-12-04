﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pronia.Models;
using Pronia.Utilites.Enums;
using Pronia.ViewModel;

namespace Pronia.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser>userManager,SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Register() //index sehfesinin yerine regist olucey, ve View qaytarmalidi
        {
            return View();
        }



        [HttpPost]

        public async Task<IActionResult> Register(RegisterVM userVM)
        {
            if(ModelState.IsValid) return View();
            
            AppUser user = new AppUser()
            {
                Name = userVM.Name,
                Surname = userVM.Surname,
                UserName = userVM.UserName,
                Email = userVM.Email,
                Gender = userVM.Gender
            };

            IdentityResult result = await _userManager.CreateAsync(user, userVM.Password);

            if(!result.Succeeded)
            {
                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(String.Empty,error.Description);
                    
                }
                return View();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index","Home");
        }

        public static bool SelectGener(GenderType type)
        {
            if(type == GenderType.Male)
            {
                return true;
            }
            if (type == GenderType.Female) return true;

            else
            {
                return false;
            }
            
        }


    }
}