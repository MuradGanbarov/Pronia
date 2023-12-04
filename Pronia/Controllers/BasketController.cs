using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModel;

namespace Pronia.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        public BasketController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<BasketItemVM> basketVM = new List<BasketItemVM>();
            if (Request.Cookies["Basket"] is not null)
            {
                List<BasketCookiesItemVM> basket = JsonConvert.DeserializeObject<List<BasketCookiesItemVM>>(Request.Cookies["Basket"]);
                foreach(var basketcookieitem in basket)
                {
                    Product product = await _context.Products.Include(p=>p.productImages.Where(pi=>pi.IsPrimary==true)).FirstOrDefaultAsync(p=>p.Id == basketcookieitem.Id);
                    if(product is not null)
                    {
                        BasketItemVM basketItemVM = new BasketItemVM
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Image = product.productImages.FirstOrDefault().URL,
                            Price = (decimal)product.Price,
                            Count = basketcookieitem.Count,
                            Subtotal = basketcookieitem.Count * (decimal)product.Price,
                        };
                        basketVM.Add(basketItemVM);
                    }
                }
                
            }

            return View(basketVM);
        }

        public async Task<IActionResult> AddBasket(int id)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
            List<BasketCookiesItemVM> basket;

            if (Request.Cookies["Basket"] is not null)
            {
                basket = JsonConvert.DeserializeObject<List<BasketCookiesItemVM>>(Request.Cookies["Basket"]);

                BasketCookiesItemVM item = basket.FirstOrDefault(b => b.Id == id);

                if (item is null)
                {
                    BasketCookiesItemVM basketCookiesItemVM = new BasketCookiesItemVM
                    {
                        Id = id,
                        Count = 1
                    };

                    basket.Add(basketCookiesItemVM);
                }
                else
                {
                    item.Count++;
                }

            }

            else
            {
                basket = new List<BasketCookiesItemVM>();

                BasketCookiesItemVM basketCookiesItemVM = new() 
                {
                    Id = id,
                    Count = 1
                };
                basket.Add(basketCookiesItemVM);
                
            }
            string json = JsonConvert.SerializeObject(basket);
            Response.Cookies.Append("Basket", json);
            return RedirectToAction(nameof(Index));
     

        }

        public async Task<IActionResult> GetBasketItems()
        {
            return Content(Request.Cookies["Basket"]);
        }

    }

}
