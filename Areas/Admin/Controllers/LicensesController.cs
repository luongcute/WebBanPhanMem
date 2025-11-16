using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Areas.Admin.Models;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminAuth", Roles = "Admin")]

    [AllowAnonymous] // Giữ nguyên AllowAnonymous theo yêu cầu của bạn
    public class LicensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LicensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        [HttpGet]
        public async Task<IActionResult> Index(int? productId)
        {
            if (!productId.HasValue)
            {
                TempData["Error"] = "Vui lòng chọn một sản phẩm để quản lý License Key.";
                // Cần đảm bảo có Controller Products/Index
                return RedirectToAction("Index", "Products");
            }

            var product = await _context.Products.FindAsync(productId.Value);

            if (product == null)
                return NotFound();

            ViewBag.ProductName = product.Name;
            ViewBag.ProductId = product.Id;

            var keys = await _context.LicenseKeys
                .Where(k => k.ProductId == productId.Value)
                .Select(k => new LicenseKeyAdminViewModel
                {
                    Id = k.Id,
                    Key = k.KeyContent,
                    IsUsed = k.IsUsed,
                    ActivatedDate = k.ActivatedDate,
                    ProductId = k.ProductId,
                    ProductName = product.Name
                })
                .OrderByDescending(k => k.Id) // Sắp xếp theo ID mới nhất
                .ToListAsync();

            return View(keys);
        }

        // ================= CREATE (GET) =================
        [HttpGet]
        public async Task<IActionResult> Create(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            ViewBag.ProductId = productId;
            ViewBag.ProductName = product.Name;

            var viewModel = new LicenseKeyAdminViewModel { ProductId = productId };
            return View(viewModel);
        }

        // ================= CREATE (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, string keysText)
        {
            // TÌM SẢN PHẨM TRƯỚC ĐỂ SỬ DỤNG CHO VIEWBAG NẾU CÓ LỖI
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            ViewBag.ProductId = productId;
            ViewBag.ProductName = product.Name;

            if (string.IsNullOrWhiteSpace(keysText))
            {
                ModelState.AddModelError("", "Danh sách key không được để trống.");

                // ĐÃ SỬA: GÁN LẠI NỘI DUNG KEY VÀO VIEWDAT
                ViewData["keysText"] = keysText;

                return View(); // Trả về View với ProductName, ProductId VÀ keysText
            }

            var lines = keysText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var newKeys = new List<LicenseKey>(); // Dùng AddRange để tối ưu DB

            foreach (var line in lines)
            {
                newKeys.Add(new LicenseKey
                {
                    KeyContent = line.Trim(),
                    ProductId = productId,
                    IsUsed = false
                });
            }

            // TỐI ƯU: Thêm tất cả các key vào Context một lần
            _context.LicenseKeys.AddRange(newKeys);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Thêm {newKeys.Count} key thành công cho sản phẩm '{product.Name}'!";
            return RedirectToAction(nameof(Index), new { productId });
        }

        // ================= DELETE =================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var key = await _context.LicenseKeys.FindAsync(id);
            if (key == null)
                return NotFound();

            if (key.IsUsed)
            {
                // CẢNH BÁO: Key này có thể đã được gán cho một Order.
                TempData["Error"] = "Không thể xóa key đã được sử dụng (đã bán hoặc kích hoạt)!";
                return RedirectToAction(nameof(Index), new { productId = key.ProductId });
            }

            _context.LicenseKeys.Remove(key);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa key thành công!";
            return RedirectToAction(nameof(Index), new { productId = key.ProductId });
        }
    }
}