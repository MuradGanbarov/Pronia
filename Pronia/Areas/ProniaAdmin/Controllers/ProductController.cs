using Microsoft.AspNetCore.Authorization;
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
    [Authorize()]
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
            if (!productVM.MainPhoto.IsValidType(FileType.Image))
            {
                productVM.Categories = await GetCategoriesAsync();
                productVM.Tags = await GetTagsAsync();
                productVM.Colors = await GetColorsAsync();
                productVM.Sizes = await GetSizesAsync();
                ModelState.AddModelError("MainPhoto", "File tipi uygun deyil:");
                return View();
            }
            if (!productVM.MainPhoto.IsValidSize(5,FileSize.Megabite))
            {
                productVM.Categories = await GetCategoriesAsync();
                productVM.Tags = await GetTagsAsync();
                productVM.Colors = await GetColorsAsync();
                productVM.Sizes = await GetSizesAsync();
                ModelState.AddModelError("Main Photo", "File olcusu uygun deyil:5mb");
                return View();
            }

            if (!productVM.HoverPhoto.IsValidType(FileType.Image))
            {
                productVM.Categories = await GetCategoriesAsync();
                productVM.Tags = await GetTagsAsync();
                productVM.Colors = await GetColorsAsync();
                productVM.Sizes = await GetSizesAsync();
                ModelState.AddModelError("Hover Photo", "File tipi uygun deyil:");
                return View();
            }
            if (!productVM.HoverPhoto.IsValidSize(5,FileSize.Megabite))
            {
                productVM.Categories = await GetCategoriesAsync();
                productVM.Tags = await GetTagsAsync();
                productVM.Colors = await GetColorsAsync();
                productVM.Sizes = await GetSizesAsync();
                ModelState.AddModelError("Hover Photo", "File olcusu uygun deyil:5mb");
                return View();
            }

            

            ProductImage image = new ProductImage
            {
                Alternative = productVM.Name,
                IsPrimary = true,
                URL = await productVM.MainPhoto.CreateAsync(_env.WebRootPath, "assets", "images", "product")
            };
            ProductImage HoverImage = new ProductImage
            {
                Alternative = productVM.Name,
                IsPrimary = false,
                URL = await productVM.HoverPhoto.CreateAsync(_env.WebRootPath, "assets", "images", "product")
            };


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
                productImages = new List<ProductImage>() { image, HoverImage}
            };




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

            foreach(IFormFile photo in productVM.Photos)
            {
                if (!photo.IsValidType(FileType.Image))
                {
                    TempData["Message"] += $"\n{photo.FileName} file tipi uygun deyil";
                    continue;
                }
                if (!photo.IsValidSize(5, FileSize.Megabite))
                {
                    TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} File olchusu uygun deyil:5mb olmalidi<p>";
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
            Product product = await _context.Products.Include(p=>p.productImages).Include(p=>p.ProductTags).Include(p=>p.ProductColors).Include(p=>p.ProductSizes).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();

            foreach (ProductImage image in product.productImages)
            {
                image.URL.Delete(_env.WebRootPath, "assets", "images", "product");
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }


        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.Include(p=>p.productImages).
                Include(p => p.ProductTags).
                Include(p => p.ProductSizes).
                Include(p => p.ProductColors).
                FirstOrDefaultAsync(p => p.Id == id);
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
                ProductImages = product.productImages
            };

            return View(productVM);

        }


        [HttpPost]

        public async Task<IActionResult> Update(int id, UpdateProductVM productVM)
        {
            Product existed = await _context.Products.Include(p => p.productImages).Include(p => p.ProductTags).Include(p => p.ProductColors).Include(p => p.ProductSizes).FirstOrDefaultAsync(p => p.Id == id);
            productVM.ProductImages = existed.productImages;
            if (!ModelState.IsValid)
            {
                productVM.Categories = await _context.Categories.ToListAsync();
                ModelState.AddModelError("Category", "Bele bir kateqoriya movcud deyil");
                productVM.Tags = await GetTagsAsync();
                ModelState.AddModelError("Tags", "Bele bir taglar movcud deyil");
                productVM.Colors = await GetColorsAsync();
                ModelState.AddModelError("Colors", "Bele bir rengler movcud deyil");
                productVM.Sizes = await GetSizesAsync();
                ModelState.AddModelError("Size", "Bele bir olchular movcud deyil");
                return View(productVM);
            }

            

            if (existed is null) return NotFound();

            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                productVM.Categories = await GetCategoriesAsync();
                productVM.Tags = await GetTagsAsync();
                productVM.Colors = await GetColorsAsync();
                productVM.Sizes = await GetSizesAsync();
                return View(productVM);
            }

            if(productVM.MainPhoto is not null)
            {
                if (!productVM.MainPhoto.IsValidType(FileType.Image))
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await GetTagsAsync();
                    productVM.Colors = await GetColorsAsync();
                    productVM.Sizes = await GetSizesAsync();
                    ModelState.AddModelError("MainPhoto", "Bele bir tip movcud deyil");
                    return View(productVM);
                }
                if (!productVM.MainPhoto.IsValidSize(5, FileSize.Megabite))
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await GetTagsAsync();
                    productVM.Colors = await GetColorsAsync();
                    productVM.Sizes = await GetSizesAsync();
                    ModelState.AddModelError("MainPhoto", "Bele bir olchusu movcud deyil:5mb");
                    return View(productVM);
                }

            }




            if(productVM.HoverPhoto is not null)
            {
                if(!productVM.HoverPhoto.IsValidType(FileType.Image))
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await GetTagsAsync();
                    productVM.Colors = await GetColorsAsync();
                    productVM.Sizes = await GetSizesAsync();
                    ModelState.AddModelError("HoverPhoto", "Bele bir tip movcud deyil");
                    return View(productVM);
                }

                if (!productVM.HoverPhoto.IsValidSize(5, FileSize.Megabite))
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await GetTagsAsync();
                    productVM.Colors = await GetColorsAsync();
                    productVM.Sizes = await GetSizesAsync();
                    ModelState.AddModelError("MainPhoto", "Bele bir olchusu movcud deyil:5mb");
                    return View(productVM);
                }
            }
            //Burda yaratmaqchun yoxluyuruq
            if(productVM.MainPhoto is not null)
            {
                string fileName = await productVM.MainPhoto.CreateAsync(_env.WebRootPath, "assets", "images", "product");
                ProductImage mainImage = existed.productImages.FirstOrDefault(pi=>pi.IsPrimary==true);
                mainImage.URL.Delete(_env.WebRootPath, "assets", "images", "product");
                _context.ProductImages.Remove(mainImage);

                existed.productImages.Add(new ProductImage
                {
                    Alternative = productVM.Name,
                    IsPrimary = true,
                    URL=fileName,
                });
            }

            if (productVM.HoverPhoto is not null)
            {
                string fileName = await productVM.HoverPhoto.CreateAsync(_env.WebRootPath, "assets", "images", "product");
                ProductImage hoverImage = existed.productImages.FirstOrDefault(pi => pi.IsPrimary == false);
                hoverImage.URL.Delete(_env.WebRootPath, "assets", "images", "product");
                _context.ProductImages.Remove(hoverImage);

                existed.productImages.Add(new ProductImage
                {
                    Alternative=productVM.Name,
                    IsPrimary = false,
                    URL=fileName,
                });
            }

            if(productVM.ImageIds is null)
            {
                productVM.ImageIds = new List<int>();
            }

            Product product = new Product
            {
                Name = productVM.Name,
                Price = productVM.Price,
                SKU = productVM.SKU,
                CategoryId = (int)productVM.CategoryId,
                Description = productVM.Description,
                ProductTags = new List<ProductTag>(),
                ProductColors = new List<ProductColor>(),
                ProductSizes = new List<ProductSize>(),
                productImages = new List<ProductImage>()
            };


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

            List<ProductImage> removeable = existed.productImages.Where(pi => !productVM.ImageIds.Exists(imgId => imgId == pi.Id) && pi.IsPrimary == null).ToList();
            foreach (ProductImage pImage in removeable)
            {
                pImage.URL.Delete(_env.WebRootPath, "assets", "images", "product");
                existed.productImages.Remove(pImage);
            }
            TempData["Message"] = "";
            if(productVM.Photos is not null)
            {
                foreach (IFormFile photo in productVM.Photos ?? new List<IFormFile>())
                {
                    if (!photo.IsValidType(FileType.Image))
                    {
                        TempData["Message"] += $"\n{photo.FileName} file tipi uygun deyil";
                        continue;
                    }
                    if (!photo.IsValidSize(5, FileSize.Megabite))
                    {
                        TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} File olchusu uygun deyil: 5mb olmalidi</p>";
                        continue;
                    }

                    existed.productImages.Add(new ProductImage
                    {
                        Alternative = productVM.Name,
                        IsPrimary = null,
                        URL = await photo.CreateAsync(_env.WebRootPath, "assets", "images", "product")
                    });

                }
            }

            






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





