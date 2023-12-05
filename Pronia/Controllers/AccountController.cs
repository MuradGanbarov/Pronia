using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pronia.Models;
using Pronia.ViewModel;

namespace Pronia.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
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

        public async Task<IActionResult> Login(LoginVM loginVM) // Bu hisse username yada email'ile axtarir
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
            if(result.IsLockedOut)
            {
                ModelState.AddModelError(String.Empty, "Please try again in 3 minutes");
                return View();
            }
            
            
            if (!result.Succeeded)
            {
                ModelState.AddModelError(String.Empty,"Incorrect username,password or email");
                return View();
                
            }


            return RedirectToAction("Index", "Home");

        }



















    }
}
