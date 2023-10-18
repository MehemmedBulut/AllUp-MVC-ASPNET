using AllUp2.DAL;
using AllUp2.Helpers;
using AllUp2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AllUp2.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public ProductsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> products= await _db.Products.
                Include(x=>x.ProductImages).
                Include(x => x.ProductDetail).
                Include(x => x.Brand).
                Include(x => x.ProductTags).
                ThenInclude(x=>x.Tag).
                Include(x => x.ProductCategories).
                ThenInclude(x=>x.Category).
                ToListAsync();
            return View(products);
        }
        #region Create
        public async Task<IActionResult> Create()
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            ViewBag.Brands = await _db.Brands.ToListAsync();
            ViewBag.Tags = await _db.Tags.ToListAsync();
            Category? firstMainCategory = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync();
            ViewBag.ChildCategories = firstMainCategory?.Children;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, int brandId, int mainCatId, int? childCatId, List<int> tagsId)
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            ViewBag.Brands = await _db.Brands.ToListAsync();
            ViewBag.Tags = await _db.Tags.ToListAsync();
            Category? firstMainCategory = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync();
            ViewBag.ChildCategories = firstMainCategory?.Children;
            if (mainCatId == null)
            {
                return NotFound();
            }
            #region isExist
            bool isExist = await _db.Products.AnyAsync(x => x.Name == product.Name);
            if (isExist)
            {
                ModelState.AddModelError("Name", "This product already is exist");
                return View();
            }
            #endregion

            #region SaveImagesFile
            if (product.Photos == null)
            {
                ModelState.AddModelError("Photo", "Please select file");
                return View();
            }
            List<ProductImage> productImages = new List<ProductImage>();
            foreach (IFormFile Photo in product.Photos)
            {
                ProductImage productImage = new ProductImage();
                if (!Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please select image file");
                    return View();
                }
                if (Photo.IsOlder1Mb())
                {
                    ModelState.AddModelError("Photo", "Max 1Mb");
                    return View();
                }
                string folder = Path.Combine(_env.WebRootPath, "assets", "images", "product");
                productImage.Url = await Photo.SaveFileAsync(folder);
                productImages.Add(productImage);
            }

            product.ProductImages = productImages;
            #endregion




            List<ProductTag> productTags = new List<ProductTag>();
            foreach (int tagId in tagsId)
            {
                ProductTag productTag = new ProductTag
                {
                    TagId = tagId,
                };
                productTags.Add(productTag);
            }
            product.ProductTags = productTags;




            List<ProductCategory> productCategories = new List<ProductCategory>();
            ProductCategory mainProductCategory = new ProductCategory
            {
                CategoryId = mainCatId,
            };
            productCategories.Add(mainProductCategory);
            if (childCatId != null)
            {
                ProductCategory childProductCategory = new ProductCategory
                {
                    CategoryId = (int)childCatId,
                };
                productCategories.Add(childProductCategory);
            }
            product.ProductCategories = productCategories;





            product.BrandId = brandId;

            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion

        public async Task<IActionResult> GetChildCategories(int mainId)
        {
            List<Category> childCategories= await _db.Categories.Where(x=>x.ParentId==mainId).ToListAsync();
            return PartialView("_GetChildCategoriesPartial", childCategories);
        }

        #region Update
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Product? dbProduct = await _db.Products.
                Include(x => x.ProductDetail).
                Include(x => x.ProductImages).
                Include(x => x.Brand).
                Include(x => x.ProductTags).
                Include(x => x.ProductCategories).
                ThenInclude(x => x.Category).
                ThenInclude(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id);
            
            if (dbProduct == null)
            {
                return BadRequest();
            }
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            ViewBag.Brands = await _db.Brands.ToListAsync();
            ViewBag.Tags = await _db.Tags.ToListAsync();
            Category mainCategory = dbProduct.ProductCategories.FirstOrDefault(x => x.Category.IsMain).Category;
            ViewBag.ChildCategories = mainCategory?.Children;
            return View(dbProduct);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Product product, int brandId, int mainCatId, int? childCatId, List<int> tagsId)
        {
            if (id == null)
            {
                return NotFound();
            }
            Product? dbProduct = await _db.Products.
                Include(x => x.ProductDetail).
                Include(x => x.ProductImages).
                Include(x => x.Brand).
                Include(x => x.ProductTags).
                Include(x => x.ProductCategories).
                ThenInclude(x => x.Category).
                ThenInclude(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (dbProduct == null)
            {
                return BadRequest();
            }
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            ViewBag.Brands = await _db.Brands.ToListAsync();
            ViewBag.Tags = await _db.Tags.ToListAsync();
            Category? firstMainCategory = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync();
            ViewBag.ChildCategories = firstMainCategory?.Children;

            if (mainCatId == null)
            {
                return NotFound();
            }
            #region isExist
            bool isExist = await _db.Products.AnyAsync(x => x.Name == product.Name && x.Id != id);
            if (isExist)
            {
                ModelState.AddModelError("Name", "This product already is exist");
                return View();
            }
            #endregion

            #region SaveImagesFile
            if (product.Photos != null)
            {
                List<ProductImage> productImages = new List<ProductImage>();
                foreach (IFormFile Photo in product.Photos)
                {
                    ProductImage productImage = new ProductImage();
                    if (!Photo.IsImage())
                    {
                        ModelState.AddModelError("Photo", "Please select image file");
                        return View();
                    }
                    if (Photo.IsOlder1Mb())
                    {
                        ModelState.AddModelError("Photo", "Max 1Mb");
                        return View();
                    }
                    string folder = Path.Combine(_env.WebRootPath, "assets", "images", "product");
                    productImage.Url = await Photo.SaveFileAsync(folder);
                    productImages.Add(productImage);
                }

                dbProduct.ProductImages.AddRange(productImages);
            }

            #endregion




            List<ProductTag> productTags = new List<ProductTag>();
            foreach (int tagId in tagsId)
            {
                ProductTag productTag = new ProductTag
                {
                    TagId = tagId,
                };
                productTags.Add(productTag);
            }
            dbProduct.ProductTags = productTags;




            List<ProductCategory> productCategories = new List<ProductCategory>();
            ProductCategory mainProductCategory = new ProductCategory
            {
                CategoryId = mainCatId,
            };
            productCategories.Add(mainProductCategory);
            if (childCatId != null)
            {
                ProductCategory childProductCategory = new ProductCategory
                {
                    CategoryId = (int)childCatId,
                };
                productCategories.Add(childProductCategory);
            }
            dbProduct.ProductCategories = productCategories;





            dbProduct.BrandId = brandId;

            dbProduct.ProductDetail = product.ProductDetail;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion

        public int DeleteImage(int id,int productId)
        {
            int count = _db.ProductImages.Count(x=>x.ProductId==productId);
            if (count == 2)
            {
                return 1;
            }
            ProductImage? productImage= _db.ProductImages.FirstOrDefault(x=>x.Id==id);
            _db.ProductImages.Remove(productImage);
            _db.SaveChanges();
            
            return 0;
        }




        #region Activity
        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Product dbProduct = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (dbProduct == null)
            {
                return BadRequest();
            }
            if (dbProduct.IsDeactive)
            {
                dbProduct.IsDeactive = false;
            }
            else
            {
                dbProduct.IsDeactive = true;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion
    }
}
