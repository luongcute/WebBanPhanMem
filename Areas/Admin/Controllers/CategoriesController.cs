using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Areas.Admin.Models;
using WebBanPhanMem.Data; // sửa theo namespace DbContext của bạn
using WebBanPhanMem.Models; // sửa theo namespace Entity Category của bạn
using System.Linq;
using System.Threading.Tasks;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Products) // Đảm bảo Products được load
                .Select(c => new CategoryAdminViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProductCount = c.Products != null ? c.Products.Count() : 0
                })
                .ToListAsync();

            return View(categories);
        }


        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = new Category
                {
                    Name = model.Name
                };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)  // Load navigation property
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            var model = new CategoryAdminViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ProductCount = category.Products != null ? category.Products.Count() : 0
            };

            return View(model);
        }


        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryAdminViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _context.Categories.FindAsync(id);
                    if (category == null)
                        return NotFound();

                    category.Name = model.Name;
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(model.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)  // load Products để tránh null
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
                return NotFound();

            var model = new CategoryAdminViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ProductCount = category.Products != null ? category.Products.Count() : 0
            };

            return View(model);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
