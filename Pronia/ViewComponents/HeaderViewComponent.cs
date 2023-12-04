using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModel;

namespace Pronia.ViewComponents
{
    public class HeaderViewComponent:ViewComponent
    {
        private readonly AppDbContext _context;

        public HeaderViewComponent(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<BasketItemVM> basketVM = new List<BasketItemVM>();
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
            //Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(p=>p.Key,p=>p.Value);
            HeaderVM headerVM = new HeaderVM
            {
                Settings = await _context.Settings.ToDictionaryAsync(p => p.Key, p => p.Value),
                BasketItems = basketVM            
            };
            return View(headerVM);
        }
    }
}
