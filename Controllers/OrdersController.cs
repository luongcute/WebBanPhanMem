using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;
using WebBanPhanMem.ViewModels;
using System.Security.Claims;

namespace WebBanPhanMem.Controllers
{
    [Authorize(AuthenticationSchemes = "UserScheme")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Orders/History
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var orderVMs = orders.Select(o => new OrderHistoryVM
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                TotalAmount = o.TotalAmount,
                Items = o.Items.Select(i => new OrderItemVM
                {
                    Id = i.Id, // cần thêm Id vào OrderItemVM
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    LicenseKey = i.LicenseKey
                }).ToList()
            }).ToList();

            return View(orderVMs);
        }

        // GET: /Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.LicenseKeys)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null) return NotFound();

            var orderVM = new OrderHistoryVM
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(i => new OrderItemVM
                {
                    Id = i.Id,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    LicenseKey = i.LicenseKey
                }).ToList()
            };

            return View(orderVM);
        }

        // POST: /Orders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null) return NotFound();

            if (order.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy đơn hàng ở trạng thái Chờ xử lý.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công!";
            return RedirectToAction(nameof(History));
        }

        // GET: /Orders/DownloadLicense/5
        public async Task<IActionResult> DownloadLicense(int orderItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(oi => oi.Id == orderItemId &&
                                         oi.Order.UserId == userId &&
                                         oi.Order.PaymentStatus == "Paid");

            if (orderItem == null || string.IsNullOrEmpty(orderItem.LicenseKey))
            {
                TempData["ErrorMessage"] = "Không tìm thấy license key hoặc đơn hàng chưa thanh toán.";
                return RedirectToAction(nameof(History));
            }

            var fileName = $"{orderItem.Product.Name.Replace(" ", "_")}_License.txt";
            var content = new System.Text.StringBuilder()
                .AppendLine($"License Key: {orderItem.LicenseKey}")
                .AppendLine($"Product: {orderItem.Product.Name}")
                .AppendLine($"Assigned: {orderItem.LicenseAssignedAt:dd/MM/yyyy HH:mm}")
                .AppendLine($"Order ID: {orderItem.OrderId}")
                .AppendLine()
                .AppendLine("--- SoftStore License ---")
                .ToString();

            return File(System.Text.Encoding.UTF8.GetBytes(content), "text/plain", fileName);
        }
    }
}
