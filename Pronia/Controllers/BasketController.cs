using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModel;
using System.Security.Claims;

namespace Pronia.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            List<BasketItemVM> basketVM = new List<BasketItemVM>();
            if (User.Identity.IsAuthenticated)
            {
                AppUser? user = await _userManager.Users.Include(u => u.BasketItems).ThenInclude(bi => bi.Product).ThenInclude(p => p.productImages.Where(pi => pi.IsPrimary == true)).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                foreach (BasketItemVM item in user.BasketItems)
                {
                    basketVM.Add(new BasketItemVM
                    {
                        Name = item.Product.Name,
                        Price = (decimal)item.Product.Price,
                        Count = item.Count,
                        Subtotal = (decimal)(item.Count * item.Product.Price),
                        Image = item.Product.productImages.FirstOrDefault()?.URL

                    });
                }


            }

            else
            {
                if (Request.Cookies["Basket"] is not null)
                {
                    List<BasketCookiesItemVM> basket = JsonConvert.DeserializeObject<List<BasketCookiesItemVM>>(Request.Cookies["Basket"]);
                    foreach (var basketcookieitem in basket)
                    {
                        Product product = await _context.Products.Include(p => p.productImages.Where(pi => pi.IsPrimary == true)).FirstOrDefaultAsync(p => p.Id == basketcookieitem.Id);
                        if (product is not null)
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
            }
            return View(basketVM);
        }

        public async Task<IActionResult> AddBasket(int id)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
            List<BasketCookiesItemVM> basket;

            if (User.Identity.IsAuthenticated)
            {
                AppUser user = await _userManager.Users.Include(u => u.BasketItems).FirstOrDefaultAsync(u => u.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (user is null) return NotFound();
                BasketItem item = user.BasketItems.FirstOrDefault(b=>b.ProductId==id);
                if(item is null)
                {
                    item = new BasketItem
                    {
                        AppUserId = user.Id,
                        ProductId = product.Id,
                        Price = (decimal)product.Price,
                        Count = 1
                    };
                    await _context.BasketItems.AddAsync(item);
                }
                else
                {
                    item.Count++;
                    
                }
                await _context.SaveChangesAsync();
            }

            else
            {
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
            return RedirectToAction(nameof(Index),"Home");
        }

        public async Task<IActionResult> GetBasketItems()
        {
            return Content(Request.Cookies["Basket"]);
        }

        public IActionResult Check()
        {
            return View();
        }

    }

}
