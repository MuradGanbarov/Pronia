using Microsoft.AspNetCore.Mvc;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModel;

namespace Pronia.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            List<Slide> slides=_context.Slides.OrderBy(s=>s.Order).Take(2).ToList();
            List<Product> products = _context.Products.OrderByDescending(s => s.Id).Take(8).ToList();

            //List<Product> products = new List<Product>
            //{
            //    new Product
            //    {
            //        Name="Rose",
            //        PrimarrImageURL="rose.jpg",
            //        SecondaryImageURL="rose2.jpg",
            //        Price = 24.46
            //    },

            //    new Product
            //    {
            //        Name="Fialka",
            //        PrimarrImageURL="Fialka.jpg",
            //        SecondaryImageURL="Fialka2.jpg",
            //        Price=13.45
            //    },

            //    new Product
            //    {
            //        Name="Orxideya",
            //        PrimarrImageURL="Orxideya.jpg",
            //        SecondaryImageURL="Orxideya2.jpg",
            //        Price=35.45
            //    },

            //    new Product
            //    {
            //        Name="Xrizontema",
            //        PrimarrImageURL="Xrizontema.jpg",
            //        SecondaryImageURL="Xrizontema2.jpg",
            //        Price=45.56
            //    },

            //    new Product
            //    {
            //        Name="Liliyi",
            //        PrimarrImageURL="Liliya.jpg",
            //        SecondaryImageURL="Liliya2.jpg",
            //        Price=67.86
            //    },

            //    new Product
            //    {
            //        Name="Pioni",
            //        PrimarrImageURL="pioni.jpg",
            //        SecondaryImageURL="pioni2.jpg",
            //        Price=47.76
            //    },

            //    new Product
            //    {
            //        Name="Nargiz",
            //        PrimarrImageURL="Nargiz.jpg",
            //        SecondaryImageURL="Nargiz2.jpg",
            //        Price=56.78
            //    },

            //    new Product
            //    {
            //        Name="Gerberi",
            //        PrimarrImageURL="Gerberi.jpg",
            //        SecondaryImageURL="Gerberi2.jpg",
            //        Price=28.50
            //    }
            //};

            //List<Slide> slides = new List<Slide>
            //{
            //    new Slide
            //    {

            //        Title="Bashliq 1",
            //        SubTitle="1ci bashliq 2",
            //        Description="Chox gozel bir slide",
            //        ImageURL="slide1.jpg",
            //        Order=2
            //    },

            //    new Slide
            //    {

            //        Title="Bashliq 2",
            //        SubTitle="2ci bashliq 3",
            //        Description="Chox gozel bir slide 2",
            //        ImageURL="slide2.jpg",
            //        Order=2
            //    },

            //    new Slide
            //    {

            //        Title="Bashliq 3",
            //        SubTitle="3cu bashliq 4",
            //        Description="Chox gozel bir slide 3",
            //        ImageURL="slide3.jpg",
            //        Order=1
            //    },

            //    new Slide
            //    {

            //        Title="Bashliq 4",
            //        SubTitle="4cu bashliq 4",
            //        Description="Chox gozel bir slide 4",
            //        ImageURL="slide4.jpg",
            //        Order=4
            //    }



            //};

            //_context.Products.AddRange(products);
            //_context.SaveChanges();


            HomeVMcs vm = new()
            {
                Slides = slides,
                Products = products,
            };

            return View(vm);
        }
        
    }
}
