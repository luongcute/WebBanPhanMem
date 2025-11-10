using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    [Area("Admin")]

    // Controller này sẽ được định tuyến bởi cấu hình trong Program.cs:
    // {area:exists}/{controller=Orders}/{action=Index}/{id?}
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Orders
        // Conventional Routing sẽ map tới: /Admin/Orders/Index
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)!
                    .ThenInclude(i => i.Product)
                .Include(o => o.Items)!
                    .ThenInclude(i => i.Product!.Category)
                .ToListAsync();

            return View(orders);
        }

        // GET: Admin/Orders/Details/5
        // Conventional Routing sẽ map tới: /Admin/Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)!
                    .ThenInclude(i => i.Product)
                .Include(o => o.Items)!
                    .ThenInclude(i => i.Product!.Category)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var user = await _context.Users.FindAsync(order.UserId);
            ViewBag.CustomerName = user?.UserName ?? "(Khách hàng ẩn danh)";

            return View(order);
        }

        // --- HÀNH ĐỘNG XÓA (AN TOÀN) ---

        // GET: Admin/Orders/Delete/5
        // Action này chỉ để hiển thị trang xác nhận xóa
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Trả về View Delete.cshtml (bạn cần tạo View này)
            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        // Action này thực hiện việc xóa dữ liệu thực tế
        [HttpPost, ActionName("Delete")] // Map nó với URL /Delete/ (dùng POST)
        [ValidateAntiForgeryToken] // Attribute bảo mật quan trọng
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order != null)
            {
                // Giả sử mối quan hệ có cấu hình xóa cascade, nếu không bạn phải xóa OrderItems trước.
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa đơn hàng!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}