using Humanizer.Localisation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.IdentityModel.Tokens;
using NuGet.Versioning;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilites.Enums;
using Pronia.ViewModel;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;

namespace Pronia.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }
        public IActionResult Register() //index sehfesinin yerine regist olucey, ve View qaytarmalidi
        {
            return View();
        }



        [HttpPost]

        public async Task<IActionResult> Register(RegisterVM userVM)
        {
            if (!ModelState.IsValid) return View();

            AppUser user = new AppUser()
            {
                Name = userVM.Name.Trim().ToUpper(),
                Surname = userVM.Surname.Trim().ToUpper(),
                UserName = userVM.UserName,
                Email = userVM.Email.Trim().ToUpper(),
                Gender = userVM.Gender.ToString()
            };

            IdentityResult result = await _userManager.CreateAsync(user, userVM.Password);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(String.Empty, error.Description);

                }
                return View();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }



        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Login(LoginVM loginVM, string? returnURL) // Bu hisse username yada email'ile axtarir
        {
            if (!ModelState.IsValid) return View();
            bool check = _context.Users.FirstOrDefault(u => u.UserName == loginVM.UsernameOrEmail || u.Email == loginVM.UsernameOrEmail) == null;

            if (check)
            {
                ModelState.AddModelError(String.Empty, "This account is not exist. Please register.");
                return View();
            }
            
            AppUser user = await _userManager.FindByNameAsync(loginVM.UsernameOrEmail);
            
            if (user is null)
            {
                user = await _userManager.FindByEmailAsync(loginVM.UsernameOrEmail);

                if (user is null)
                {
                    ModelState.AddModelError(String.Empty, "Incorrect username,password or email");
                    return View();
                }
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.IsRemembered, true); //Bu hisse password'u yoxluyur

            

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(String.Empty, "Please try again in 3 minutes");
                return View();
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(String.Empty, "Incorrect username,password or email");
                return View();

            }
            if (returnURL is null)
            {
                return RedirectToAction("Home", "Index");
            }
            return Redirect(returnURL);


        }



        //public async Task<IActionResult> CreateRole()
        //{
        //    foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
        //    {
        //        if (!(await _roleManager.RoleExistsAsync(role.ToString())))
        //            await _roleManager.CreateAsync(new IdentityRole
        //            {
        //                Name = role.ToString(),
        //            });
        //    }

        //    return RedirectToAction("Index", "Home");
        //}



















    }
}
