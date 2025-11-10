using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Areas.Admin.Models;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    [Area("Admin")    ]
        [AllowAnonymous]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ===================== INDEX =====================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductAdminViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : "(Chưa có danh mục)"
                })
                .ToListAsync();

            return View(products);
        }

        // ===================== CREATE (GET) =====================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // ===================== CREATE (POST) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductAdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(model);
            }

            string? fileUrl = null;

            if (model.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                fileUrl = "/uploads/products/" + fileName;
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description ?? "",
                Price = model.Price,
                CategoryId = model.CategoryId,
                ImageUrl = fileUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ===================== EDIT (GET) =====================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            var model = new ProductAdminViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId
            };

            ViewBag.Categories = _context.Categories.ToList();
            return View(model);
        }

        // ===================== EDIT (POST) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductAdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(model);
            }

            var product = await _context.Products.FindAsync(model.Id);
            if (product == null)
                return NotFound();

            if (model.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    string oldPath = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                product.ImageUrl = "/uploads/products/" + fileName;
            }

            product.Name = model.Name;
            product.Description = model.Description ?? "";
            product.Price = model.Price;
            product.CategoryId = model.CategoryId;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ===================== DELETE =====================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
