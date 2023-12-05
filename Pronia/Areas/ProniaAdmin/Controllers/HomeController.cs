using Microsoft.AspNetCore.Mvc;
using Pronia.Utilites.Enums;
using static Pronia.Areas.ProniaAdmin.Models.Utilities.Extensions.AuthorizeRoles;

namespace Pronia.Areas.Admin.Controllers
{
    [Area("ProniaAdmin")]
    [AuthorizeRolesAttribute(UserRole.Admin, UserRole.Moderator)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
