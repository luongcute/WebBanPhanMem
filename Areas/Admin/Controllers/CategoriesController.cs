using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Areas.Admin.Models;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;
using System.Linq;
using System.Threading.Tasks;
using WebBanPhanMem.Extensions;  // Extension method namespace
using Microsoft.AspNetCore.Authorization;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminAuth", Roles = "Admin")]


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
                .Include(c => c.Products)
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
        public IActionResult Create() => View();

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = new Category { Name = model.Name };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            var model = new CategoryAdminViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ProductCount = category.Products?.Count() ?? 0
            };
            return View(model);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryAdminViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null) return NotFound();

                category.Name = model.Name;
                _context.Update(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null) return NotFound();

            var model = new CategoryAdminViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ProductCount = category.Products?.Count() ?? 0
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

        // POST: Admin/Categories/DeleteAjax/{id} - Xóa danh mục qua Ajax
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Categories/DeleteConfirmed/{id}")]
        public async Task<IActionResult> DeleteConfirmedAjax(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            if (category.Products != null && category.Products.Any())
                return BadRequest("Không thể xóa danh mục đang có sản phẩm.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // Ví dụ: Xuất danh sách categories thành chuỗi HTML (dùng extension RenderViewAsync)
        public async Task<IActionResult> ExportCategoriesHtml()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .Select(c => new CategoryAdminViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProductCount = c.Products != null ? c.Products.Count() : 0
                })
                .ToListAsync();

            var htmlContent = await this.RenderViewAsync("Index", categories);

            return Content(htmlContent, "text/html");
        }

        private bool CategoryExists(int id) =>
            _context.Categories.Any(e => e.Id == id);
    }
}
