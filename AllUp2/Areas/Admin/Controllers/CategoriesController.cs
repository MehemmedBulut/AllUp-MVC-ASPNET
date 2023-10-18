using AllUp2.DAL;
using AllUp2.Helpers;
using AllUp2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUp2.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public CategoriesController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _db.Categories.Include(x => x.Parent).Include(x => x.Children).ToListAsync();
            return View(categories);
        }
        #region Create
        public async Task<IActionResult> Create()
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, int mainCatId)
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            if (category.IsMain)
            {
                #region isExist
                bool isExist = await _db.Categories.AnyAsync(x => x.Name == category.Name);
                if (isExist)
                {
                    ModelState.AddModelError("Name", "This category already is exist");
                    return View();
                }
                #endregion
                #region SaveImageFile
                if (category.Photo == null)
                {
                    ModelState.AddModelError("Photo", "Please select file");
                    return View();
                }
                if (!category.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please select image file");
                    return View();
                }
                if (category.Photo.IsOlder1Mb())
                {
                    ModelState.AddModelError("Photo", "Max 1Mb");
                    return View();
                }
                string folder = Path.Combine(_env.WebRootPath, "assets", "images");
                category.Image = await category.Photo.SaveFileAsync(folder);
                #endregion
            }
            else
            {
                category.ParentId = mainCatId;
            }
            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion

        #region Update
        public async Task<IActionResult> Update(int? id)
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            if (id == null)
            {
                return NotFound();
            }
            Category dbCategory = await _db.Categories.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (dbCategory == null)
            {
                return BadRequest();
            }
            return View(dbCategory);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Category category, int? mainCatId)
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            if (id == null)
            {
                return NotFound();
            }
            Category? dbCategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (dbCategory == null)
            {
                return BadRequest();
            }
            if (dbCategory.IsMain)
            {
                #region exist
                bool isexist = await _db.Categories.AnyAsync(x => x.Name == category.Name && dbCategory.Id != id);
                if (isexist)
                {

                    ModelState.AddModelError("name", "this service already exist");
                    return View(dbCategory);
                }
                #endregion
                if (category.Photo != null)
                {
                    if (!category.Photo.IsImage())
                    {
                        ModelState.AddModelError("Photo", "Please Upload a Photo");
                        return View();
                    }
                    if (category.Photo.IsOlder1Mb())
                    {
                        ModelState.AddModelError("Photo", "Max 1mb");
                        return View();
                    }
                    string folder = Path.Combine(_env.WebRootPath, "assets", "images");
                    string fullPath = Path.Combine(folder, dbCategory.Image);

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    dbCategory.Image = await category.Photo.SaveFileAsync(folder);

                }


            }
            else
            {
                dbCategory.ParentId = mainCatId;
            }

            dbCategory.Name = category.Name;


            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        #endregion

        #region Detail
        public async Task<IActionResult> Detail(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            Category dbCategory = await _db.Categories
                .Include(x => x.Children).Include(x => x.Parent).FirstOrDefaultAsync(x => x.Id == id);
            if (dbCategory == null)
            {
                return BadRequest();
            }
            return View(dbCategory);
        }
        #endregion

        #region Activity
        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Category dbCategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (dbCategory == null)
            {
                return BadRequest();
            }
            if (dbCategory.IsDeactive)
            {
                dbCategory.IsDeactive = false;
            }
            else
            {
                dbCategory.IsDeactive = true;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion

        #region Search
        public async Task<IActionResult> Search(string key)
        {
            List<Category> categories = await _db.Categories.Where(x => x.Name.Contains(key)).ToListAsync();
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return PartialView("_SearchCategoryPartial", categories);
        }
        #endregion

    }
}
