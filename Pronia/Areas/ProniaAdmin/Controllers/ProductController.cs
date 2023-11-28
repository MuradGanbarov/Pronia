using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.ProniaAdmin.Models;
using Pronia.Areas.ProniaAdmin.Models.Utilities.Enums;
using Pronia.Areas.ProniaAdmin.ViewModels;
using Pronia.DAL;
using Pronia.Models;


namespace Pronia.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env= env;
        }
        public async Task<IActionResult> Index()
        {
            List<Product> products = await _context.Products.Include(p => p.Category).
                Include(p => p.productImages.
                Where(pi => pi.IsPrimary == true)).
                Include(p => p.ProductTags).ThenInclude(p => p.Tag).ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {

            CreateProductVM productVM = new CreateProductVM
            {
                Categories = await GetCategoriesAsync(),
                Tags = await GetTagsAsync(),
                Colors = await GetColorsAsync(),
                Sizes = await GetSizesAsync(),
            };
            return View(productVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {

            if (!ModelState.IsValid)
            {
                productVM.Categories = await GetCategoriesAsync();
                productVM.Tags = await GetTagsAsync();
                productVM.Colors = await GetColorsAsync();
                productVM.Sizes = await GetSizesAsync();
                return View(productVM);
            }

            if (!productVM.MainPhoto.IsValidType(FileType.Image))
            { 
                productVM.Categories=await GetCategoriesAsync();
                productVM.Tags=await GetTagsAsync();
                productVM.Colors = await GetColorsAsync();
                productVM.Sizes = await GetSizesAsync();
                ModelState.AddModelError("MainPhoto","File tipi uygun deyil:");
                return View();
            }
            if (!productVM.MainPhoto.IsValidSize(600))
            {
                productVM.Categories=await GetCategoriesAsync();
                productVM.Tags=await GetTagsAsync();
                productVM.Colors = await GetColorsAsync();
                productVM.Sizes = await GetSizesAsync();
                ModelState.AddModelError("Main Photo", "File olcusu uygun deyil:600");
                return View();
            }

            if (!productVM.HoverPhoto.IsValidType(FileType.Image))
            {
                productVM.Categories = await GetCategoriesAsync();
                productVM.Tags=await GetTagsAsync();
                productVM.Colors=await GetColorsAsync();
                productVM.Sizes=await GetSizesAsync();
                ModelState.AddModelError("Hover Photo", "File tipi uygun deyil:");
                return View();
            }
            if (!productVM.HoverPhoto.IsValidSize(600))
            {
                productVM.Categories=await GetCategoriesAsync();
                productVM.Tags=await GetTagsAsync();
                productVM.Colors=await GetColorsAsync();
                productVM.Sizes=await GetSizesAsync();
                ModelState.AddModelError("Hover Photo", "File olcusu uygun deyil:600");
                return View();
            }


            ProductImage image = new ProductImage
            {
                Alternative = productVM.Name,
                IsPrimary =true,
                URL = await productVM.MainPhoto.CreateAsync(_env.WebRootPath, "assets", "images", "product")
            };
            ProductImage HoverImage = new ProductImage
            {
                Alternative = productVM.Name,
                IsPrimary = false,
                URL = await productVM.HoverPhoto.CreateAsync(_env.WebRootPath,"assets","images", "product" )
            };

            bool result = await _context.Products.AnyAsync(p => p.CategoryId == productVM.CategoryID);

            if (!result)
            {
                productVM.Categories = await GetCategoriesAsync();
                ModelState.AddModelError("CategoryID", "Bele bir kateqoriya movcud deyil");
                productVM.Tags = await GetTagsAsync();
                ModelState.AddModelError("TagsIds", "Yanlish tag melumatlari gonderilib");
                productVM.Colors = await GetColorsAsync();
                ModelState.AddModelError("ColorIds", "Yanlish color melumatlari gonderilib");
                productVM.Sizes = await GetSizesAsync();
                ModelState.AddModelError("SizeIds", "Yanlish size melumatlari gonderilib");
                return View();
            }

            foreach (int TagId in productVM.TagIds)
            {
                bool tagResult = await _context.Tags.AnyAsync(t => t.Id == TagId);
                if (!tagResult)
                {
                    productVM.Categories = await GetCategoriesAsync();
                    productVM.Tags = await GetTagsAsync();
                    ModelState.AddModelError("TagsIds", "Yanlish tag melumatlari gonderilib");
                    return View();
                }
            }

            foreach (int ColorId in productVM.ColorIds)
            {
                bool colorResult = await _context.Colors.AnyAsync(c => c.Id == ColorId);
                if (!colorResult)
                {
                    productVM.Categories = await GetCategoriesAsync();
                    productVM.Colors = await GetColorsAsync();
                    ModelState.AddModelError("ColorIds", "Yanlish color melumatlari gonderilib");
                    return View();
                }
            }

            foreach (int SizeId in productVM.SizeIds)
            {
                bool sizeResult = await _context.Sizes.AnyAsync(s => s.Id == SizeId);
                if (!sizeResult)
                {
                    productVM.Categories = await GetCategoriesAsync();
                    productVM.Sizes = await GetSizesAsync();
                    ModelState.AddModelError("SizeIds", "Yanlish size melumatlari gonderilib");
                    return View();
                }
            }

            Product product = new Product
            {
                Name = productVM.Name,
                SKU = productVM.SKU,
                Price = productVM.Price,
                CategoryId = productVM.CategoryID,
                Description = productVM.Description,
                ProductTags = new List<ProductTag>(),
                ProductColors = new List<ProductColor>(),
                ProductSizes = new List<ProductSize>(),
                productImages = new List<ProductImage>()
            };



            foreach (int tagId in productVM.TagIds)
            {
                ProductTag productTag = new ProductTag
                {
                    TagId = tagId,
                };

                product.ProductTags.Add(productTag);
            }

            foreach (int ColorId in productVM.ColorIds)
            {
                ProductColor productColor = new ProductColor
                {
                    ColorId = ColorId,
                };

                product.ProductColors.Add(productColor);
            }

            foreach (int Id in productVM.SizeIds)
            {
                ProductSize productSize = new ProductSize
                {
                    SizeId = Id,
                };

                product.ProductSizes.Add(productSize);
            }

            TempData["Message"] = "";

            foreach(IFormFile photo in productVM.Photos)
            {
                if (!photo.IsValidType(FileType.Image))
                {
                    TempData["Message"] += $"\n{photo.FileName} file tipi uygun deyil";
                    continue;
                }
                if (!photo.IsValidSize(600))
                {
                    TempData["Message"] += $"\n{photo.FileName} File olchusu uygun deyil:600";
                    continue;
                }

                product.productImages.Add(new ProductImage
                {
                    Alternative = productVM.Name,
                    IsPrimary = null,
                    URL= await photo.CreateAsync(_env.WebRootPath,"assets","images","product")
                });

            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            Product existed = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (existed == null) return NotFound();
            _context.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.Include(p => p.ProductTags).Include(p => p.ProductSizes).Include(p => p.ProductColors).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM
            {
                Name = product.Name,
                SKU = product.SKU,
                Price = product.Price,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Categories = await GetCategoriesAsync(),
                TagIds = product.ProductTags.Select(t => t.TagId).ToList(),
                Tags = await GetTagsAsync(),
                ColorIds = product.ProductColors.Select(t => t.ColorId).ToList(),
                Colors = await GetColorsAsync(),
                SizeIds = product.ProductSizes.Select(t => t.SizeId).ToList(),
                Sizes = await GetSizesAsync(),
            };

            return View(productVM);

        }


        [HttpPost]

        public async Task<IActionResult> Update(int id, UpdateProductVM productVM)
        {
            if (!ModelState.IsValid)
            {
                productVM.Categories = await _context.Categories.ToListAsync();
                productVM.Tags = await GetTagsAsync();
                productVM.Colors = await GetColorsAsync();
                productVM.Sizes = await GetSizesAsync();
                return View();
            }

            Product existed = await _context.Products.Include(p => p.ProductTags).Include(p => p.ProductColors).Include(p => p.ProductSizes).FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();

            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                productVM.Categories = await GetCategoriesAsync();
                productVM.Tags = await GetTagsAsync();
                productVM.Colors = await GetColorsAsync();
                productVM.Sizes = await GetSizesAsync();
                return View();
            }

            foreach (ProductTag proTag in existed.ProductTags)
            {
                if (!productVM.TagIds.Exists(tId => tId == proTag.TagId))
                {
                    _context.ProductTags.Remove(proTag);
                }
            }

            List<int> newTagIds = new List<int>();
            foreach (int tagId in productVM.TagIds)
            {
                if (!existed.ProductTags.Any(pt => pt.TagId == tagId))
                {
                    existed.ProductTags.Add(new ProductTag
                    {
                        TagId = tagId,
                    });

                }
            }

            foreach (ProductColor procolor in existed.ProductColors)
            {
                if (!productVM.ColorIds.Exists(cId => cId == procolor.ColorId))
                {
                    _context.Remove(procolor);
                }
            }

            List<int> NewProColors = new List<int>();

            foreach (int colorId in productVM.ColorIds)
            {
                if (!existed.ProductColors.Any(pt => pt.ColorId == colorId))
                {
                    existed.ProductColors.Add(new ProductColor
                    {
                        ColorId = colorId,
                    });
                }
            }

            foreach (ProductSize prosize in existed.ProductSizes)
            {
                if (!productVM.SizeIds.Exists(sId=>sId == prosize.Id))
                {
                    _context.Remove(prosize);
                }
            }

            List<int> NewProductSizes = new();
            foreach (int sizeId in productVM.SizeIds)
            {
                if (!existed.ProductSizes.Any(pt => pt.SizeId == sizeId))
                {
                    existed.ProductSizes.Add(new ProductSize
                        {
                        SizeId = sizeId,
                    } );
                }
            }


            existed.Name = productVM.Name;
            existed.Description = productVM.Description;
            existed.SKU = productVM.SKU;
            existed.Price = (double)productVM.Price;
            existed.CategoryId = (int)productVM.CategoryId;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.Include(p => p.productImages.Where(pi => pi.IsPrimary == true)).Include(p => p.Category).Include(p => p.ProductTags).ThenInclude(pt => pt.Tag).Include(p => p.ProductColors).ThenInclude(pc => pc.Color).Include(p => p.ProductSizes).ThenInclude(ps => ps.Size).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }


        public async Task<List<Category>> GetCategoriesAsync()
        {
            List<Category> categories = await _context.Categories.ToListAsync();
            return categories;
        }

        public async Task<List<Tag>> GetTagsAsync()
        {
            List<Tag> tags = await _context.Tags.ToListAsync();
            return tags;
        }

        public async Task<List<Color>> GetColorsAsync()
        {
            List<Color> colors = await _context.Colors.ToListAsync();
            return colors;
        }

        public async Task<List<Size>> GetSizesAsync()
        {
            List<Size> sizes = await _context.Sizes.ToListAsync();
            return sizes;
        }




    }











}





