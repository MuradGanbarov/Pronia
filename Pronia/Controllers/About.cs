using Microsoft.AspNetCore.Mvc;

namespace Pronia.Controllers
{
    public class About : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
