using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;
using WebBanPhanMem.Models.ViewModels;
using WebBanPhanMem.Services;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminAuth", Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILicenseService _licenseService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            ApplicationDbContext context,
            ILicenseService licenseService,
            ILogger<OrdersController> logger)
        {
            _context = context;
            _licenseService = licenseService;
            _logger = logger;
        }

        // ================================================================
        // INDEX – Danh sách + thống kê
        // ================================================================
        public async Task<IActionResult> Index(string statusFilter, string paymentFilter, string searchString)
        {
            try
            {
                var allOrders = _context.Orders.AsQueryable();

                var model = new OrdersIndexViewModel
                {
                    TotalOrders = await allOrders.CountAsync(),
                    PendingOrders = await allOrders.CountAsync(o => (o.Status ?? "") == "pending"),
                    PaidOrders = await allOrders.CountAsync(o => (o.PaymentStatus ?? "") == "paid"),
                    CompletedOrders = await allOrders.CountAsync(o => (o.Status ?? "") == "completed"),
                    CancelledOrders = await allOrders.CountAsync(o => (o.Status ?? "") == "cancelled"),
                };

                var query = _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Include(o => o.ApplicationUser)
                    .AsQueryable();

                // Normalize filters
                var statusNormalized = string.IsNullOrWhiteSpace(statusFilter) ? null : statusFilter.Trim().ToLowerInvariant();
                var paymentNormalized = string.IsNullOrWhiteSpace(paymentFilter) ? null : paymentFilter.Trim().ToLowerInvariant();

                if (!string.IsNullOrEmpty(statusNormalized))
                {
                    // use normalized compare avoiding ToLower in-expression
                    query = query.Where(o => (o.Status ?? "").ToLower() == statusNormalized);
                }

                if (!string.IsNullOrEmpty(paymentNormalized))
                {
                    query = query.Where(o => (o.PaymentStatus ?? "").ToLower() == paymentNormalized);
                }

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    var keyword = searchString.Trim();

                    // Use EF.Functions.Like and null-coalescing/ternary so EF can translate and avoid null deref
                    query = query.Where(o =>
                        EF.Functions.Like((o.CustomerName ?? ""), $"%{keyword}%") ||
                        EF.Functions.Like((o.CustomerEmail ?? ""), $"%{keyword}%") ||
                        EF.Functions.Like(o.ApplicationUser != null ? (o.ApplicationUser.Email ?? "") : "", $"%{keyword}%") ||
                        EF.Functions.Like(o.ApplicationUser != null ? (o.ApplicationUser.FullName ?? "") : "", $"%{keyword}%")
                    );
                }

                model.TotalRevenue = await query
                    .Where(o => (o.PaymentStatus ?? "") == "paid" && o.Items != null)
                    .SelectMany(o => o.Items)
                    .SumAsync(i => (decimal?)(i.Price * i.Quantity)) ?? 0m;

                model.Orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                model.CurrentPaymentFilter = paymentFilter;
                model.CurrentStatusFilter = statusFilter;
                model.CurrentSearchString = searchString;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi load danh sách đơn hàng.");
                TempData["ErrorMessage"] = "Lỗi hệ thống.";
                return View(new OrdersIndexViewModel());
            }
        }

        // ================================================================
        // DETAILS – Xem chi tiết
        // ================================================================
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Include(o => o.ApplicationUser)
                .Include(o => o.LicenseKeys).ThenInclude(k => k.Product)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        // ================================================================
        // MARK AS PAID – xác nhận thanh toán + gán key
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            if ((order.PaymentStatus ?? "").ToLowerInvariant() == "paid")
            {
                TempData["WarningMessage"] = "Đơn hàng này đã được thanh toán.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var assignedKeys = await _licenseService.AssignKeysToOrderAsync(order.Id) ?? new List<LicenseKey>();

                order.PaymentStatus = "paid";
                order.PaymentDate = DateTime.Now;

                if ((order.Status ?? "").ToLowerInvariant() == "pending")
                    order.Status = "completed";

                _context.Update(order);
                await _context.SaveChangesAsync();

                var email = order.ApplicationUser?.Email ?? order.CustomerEmail;

                if (!string.IsNullOrEmpty(email) && assignedKeys.Any())
                {
                    try
                    {
                        await _licenseService.SendKeysByEmailAsync(email, id, assignedKeys);
                        TempData["SuccessMessage"] = $"Đã xác nhận thanh toán và gửi {assignedKeys.Count} key.";
                    }
                    catch (Exception exSend)
                    {
                        _logger.LogError(exSend, "Lỗi gửi email key cho đơn #{OrderId}", id);
                        TempData["WarningMessage"] = $"Đã xác nhận thanh toán nhưng lỗi khi gửi email key đến {email}.";
                    }
                }
                else
                {
                    TempData["SuccessMessage"] = "Đã xác nhận thanh toán.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi mark paid cho đơn #{OrderId}", id);
                TempData["ErrorMessage"] = "Lỗi hệ thống.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // ================================================================
        // RESEND LICENSE KEY
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendLicenseKey(int id)
        {
            var order = await _context.Orders
                .Include(o => o.LicenseKeys).ThenInclude(k => k.Product)
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            var assignedKeys = order.LicenseKeys?.ToList() ?? new List<LicenseKey>();

            if ((order.PaymentStatus ?? "").ToLowerInvariant() != "paid" || !assignedKeys.Any())
            {
                TempData["ErrorMessage"] = "Đơn hàng chưa thanh toán hoặc chưa gán key.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var email = order.ApplicationUser?.Email ?? order.CustomerEmail;
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Không có email của khách.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                await _licenseService.SendKeysByEmailAsync(email, id, assignedKeys);
                TempData["SuccessMessage"] = $"Đã gửi lại key cho {email}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi gửi lại key cho đơn #{OrderId}", id);
                TempData["ErrorMessage"] = "Lỗi hệ thống khi gửi email.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // ================================================================
        // UPDATE STATUS (FORM POST – KHÔNG PHẢI AJAX)
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int OrderId, string? Status)
        {
            try
            {
                var validStatuses = new[] { "pending", "confirmed", "shipping", "completed", "cancelled" };
                var newStatus = (Status ?? "").Trim().ToLowerInvariant();

                if (string.IsNullOrEmpty(newStatus) || !validStatuses.Contains(newStatus))
                {
                    TempData["ErrorMessage"] = "Trạng thái không hợp lệ.";
                    return RedirectToAction(nameof(Details), new { id = OrderId });
                }

                var order = await _context.Orders.FindAsync(OrderId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction(nameof(Index));
                }

                order.Status = newStatus;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật trạng thái thành công.";
                return RedirectToAction(nameof(Details), new { id = OrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi update status cho order #{OrderId}", OrderId);
                TempData["ErrorMessage"] = "Lỗi hệ thống.";
                return RedirectToAction(nameof(Details), new { id = OrderId });
            }
        }




        // ================================================================
        // DELETE → soft delete
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            order.Status = "cancelled";

            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã hủy đơn hàng #{id}.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ================================================================
    // VIEW MODEL
    // ================================================================
    public class OrdersIndexViewModel
    {
        public List<Order> Orders { get; set; } = new();
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PaidOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public string? CurrentStatusFilter { get; set; }
        public string? CurrentPaymentFilter { get; set; }
        public string? CurrentSearchString { get; set; }
    }
}
