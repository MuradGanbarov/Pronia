using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModel;
using System.Security.Claims;

namespace Pronia.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly UserManager<AppUser> _userManager;

        public HeaderViewComponent(AppDbContext context, IHttpContextAccessor http, UserManager<AppUser> userManager)
        {
            _context = context;
            _http = http;
            _userManager = userManager;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<BasketItemVM> basketVM = new List<BasketItemVM>();
            if (_http.HttpContext.User.Identity.IsAuthenticated)
            {

                var user = await _userManager.Users.
                    Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).
                    ThenInclude(bi => bi.Product).
                    ThenInclude(p => p.productImages.
                    Where(pi => pi.IsPrimary == true)).
                    FirstOrDefaultAsync(u => u.Id == _http.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                foreach (var item in user.BasketItems)
                {
                    basketVM.Add(new BasketItemVM
                    {

                        Id = item.ProductId,
                        Name = item.Product.Name,
                        Price = item.Product.Price,
                        Count = item.Count,
                        Subtotal = (item.Count * item.Product.Price),
                        Image = item.Product.productImages.FirstOrDefault().URL

                    });

                }
            }

            else
            {

                if (_http.HttpContext.Request.Cookies["Basket"] is not null)
                {
                    List<BasketCookiesItemVM> basket = JsonConvert.DeserializeObject<List<BasketCookiesItemVM>>(_http.HttpContext.Request.Cookies["Basket"]);

                    foreach (var basketCookieItem in basket)
                    {
                        Product product = await _context.Products.Include(p => p.productImages.Where(pi => pi.IsPrimary == true)).FirstOrDefaultAsync(p => p.Id == basketCookieItem.Id);
                        if (product is not null)
                        {
                            BasketItemVM basketItemVM = new BasketItemVM
                            {
                                Id = product.Id,
                                Name = product.Name,
                                Image = product.productImages.FirstOrDefault().URL,
                                Price = product.Price,
                                Count = basketCookieItem.Count,
                                Subtotal = product.Price * basketCookieItem.Count
                            };
                            basketVM.Add(basketItemVM);
                        }
                    }
                }
            }


            HeaderVM headerVM = new HeaderVM
            {
                Settings = await _context.Settings.ToDictionaryAsync(p => p.Key, p => p.Value),
                BasketItems = basketVM
            };
            return View(headerVM);
        }
    }
}

