using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using NuGet.ContentModel;
using Pronia.Areas.ProniaAdmin.Models;
using Pronia.DAL;
using Pronia.Interfaces;
using Pronia.Models;
using Pronia.ViewModel;
using System.Security.Claims;
using System.Security.Principal;

namespace Pronia.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager, IWebHostEnvironment env, IEmailService emailService)
        {
            _context = context;
            //_http = http,
            _userManager = userManager;
            _env = env;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            List<BasketItemVM> basketVM = new List<BasketItemVM>();
            if (User.Identity.IsAuthenticated)
            {
                AppUser? user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).ThenInclude(bi => bi.Product).ThenInclude(p => p.productImages.Where(pi => pi.IsPrimary == true)).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                foreach (BasketItem item in user.BasketItems)
                {
                    basketVM.Add(new BasketItemVM
                    {
                        Id = item.Product.Id,
                        Name = item.Product.Name,
                        Price = item.Product.Price,
                        Count = item.Count,
                        Subtotal = (item.Count * item.Product.Price),
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
                                Price = product.Price,
                                Count = basketcookieitem.Count,
                                Subtotal = basketcookieitem.Count * product.Price,
                            };
                            basketVM.Add(basketItemVM);
                        }
                    }

                }
            }
            return View(basketVM);
        }

        public async Task<IActionResult> AddBasket(int id,string? returnurl)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
            List<BasketCookiesItemVM> basket;

            if (User.Identity.IsAuthenticated)
            {
                AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).FirstOrDefaultAsync(u => u.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (user is null) return NotFound();
                BasketItem item = user.BasketItems.FirstOrDefault(b => b.ProductId == id);
                if (item is null)
                {
                    item = new BasketItem
                    {
                        AppUserId = user.Id,
                        ProductId = product.Id,
                        Price = product.Price,
                        Count = 1,
                        OrderId = null,
                    };

                    await _context.BasketItems.AddAsync(item);
                }
                else
                {
                    item.Count++;
                    _context.BasketItems.Update(item);

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
                        await _context.SaveChangesAsync();
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
                if (returnurl is not null) return Redirect(returnurl);
            }
            if (returnurl is not null) return Redirect(returnurl);            
            return RedirectToAction(nameof(Index), "Home");
        }

        

        public async Task<IActionResult> Checkout()
        {
            if (User.Identity.IsAuthenticated)
            {
                AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).ThenInclude(bi => bi.Product).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
                OrderVM orderVM = new OrderVM
                {
                    BasketItems = user.BasketItems,
                };
                return View(orderVM);
            }

            else
            {
                TempData["Message"] = $"<div class=\"alert alert-danger\" role=\"alert\">\r\n You need to login for checkout!\r\n</div>";
                return RedirectToAction(nameof(Index), "Home");

            }

        }

        [HttpPost]

        public async Task<IActionResult> Checkout(OrderVM orderVM)
        {
            AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).ThenInclude(bi => bi.Product).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (!ModelState.IsValid)
            {
                orderVM.BasketItems = user.BasketItems;
                return View(orderVM);
            }

            decimal total = 0;
            foreach (BasketItem item in user.BasketItems)
            {
                item.Price = item.Product.Price;
                total += item.Count * item.Price;
            }



            Order order = new Order
            {
                Status = null,
                Address = orderVM.Address,
                PurchaseAt = DateTime.Now,
                AppUserId = user.Id,
                BasketItems = user.BasketItems,
                TotalPrice = total,

            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            string body = @"<table border=""1"">
                           <thead>
                              <tr>    
                                    <th>Name</th>    
                                    <th>Price</th>    
                                    <th>Count</th>    
                                </tr>    
                            </thead>
                            <tbody>";
            foreach (var item in order.BasketItems)
            {
                body += @$" <tr>
                                    <td>{item.Product.Name}</td>
                                    <td>{item.Price}</td>
                                    <td>{item.Count}</td>
                            </tr>";
    
            }

            body += @"</tbody>
                  </table>";

            await _emailService.SendMailAsync(user.Email, "Your Order", body, true);

            return RedirectToAction(nameof(Index), "Home");
        }



        public async Task<IActionResult> Delete(int id,string? returnurl)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if(product == null) return NotFound();
            if (User.Identity.IsAuthenticated)
            {
                BasketItem item = await _context.BasketItems.FirstOrDefaultAsync(bi => bi.ProductId == id && bi.AppUserId==User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (item is null) return NotFound();
                _context.BasketItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            else
            {
                if (Request.Cookies["Basket"] != null)
                {

                    List<BasketCookiesItemVM> basket = JsonConvert.DeserializeObject<List<BasketCookiesItemVM>>(Request.Cookies["Basket"]);
                    BasketCookiesItemVM item = basket.FirstOrDefault(b => b.Id == id);
                    basket.Remove(item);
                    string json = JsonConvert.SerializeObject(basket);
                    Response.Cookies.Append("Basket", json);
                }
            }
            if(returnurl is not null) return Redirect(returnurl);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DecreaseQuantity(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                BasketItem? item = await _context.BasketItems.FirstOrDefaultAsync(bi => bi.ProductId == id && bi.AppUserId == User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (item == null) return NotFound();
                if (item.Count > 1)
                {
                    item.Count--;
                }
                else
                {
                    _context.BasketItems.Remove(item);
                }
                await _context.SaveChangesAsync();


            }

            else
            {
                string Cookies = Request.Cookies["Basket"];

                if (Cookies.IsNullOrEmpty()) return NotFound();

                List<BasketCookiesItemVM> items = JsonConvert.DeserializeObject<List<BasketCookiesItemVM>>(Cookies);
                BasketCookiesItemVM decreaseitem = items.FirstOrDefault(i => i.Id == id);
                if (decreaseitem is null) return NotFound();
                if(decreaseitem.Count > 1)
                {
                    decreaseitem.Count--;
                    Response.Cookies.Append("Basket", JsonConvert.SerializeObject(items));
                    return RedirectToAction(nameof(Index));
                }

                items.Remove(decreaseitem);
                Response.Cookies.Append("Basket", JsonConvert.SerializeObject(items));

            }
                return RedirectToAction(nameof(Index));

        }






    }

}
