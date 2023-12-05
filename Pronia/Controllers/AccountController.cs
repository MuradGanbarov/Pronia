using Microsoft.AspNetCore.Identity;
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
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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

        public async Task<IActionResult> CreateRole() //Db'ye role'lari doldurmaq metodu
        {
            foreach(UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = role.ToString(),
                });
            }

            return RedirectToAction("Index", "Home");
        }



















    }
}
