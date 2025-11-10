using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Data;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    [Area("Admin")]
   // [Authorize(Roles = "Admin")] // YÊU CẦU NGƯỜI DÙNG PHẢI ĐĂNG NHẬP VÀ CÓ ROLE "Admin"
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Các truy vấn thống kê
            var totalProducts = await _context.Products.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // Truy vấn doanh thu theo tháng (Lấy 6 tháng gần nhất)
            var monthlyRevenueRaw = await _context.Orders
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Take(6)
                .ToListAsync();

            var monthlyRevenue = monthlyRevenueRaw
                .Select(x => new
                {
                    Month = $"{x.Month}/{x.Year}",
                    Total = x.Total
                })
                .ToList();

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalRevenue = totalRevenue.ToString("N0") + " ₫";
            ViewBag.MonthlyRevenue = monthlyRevenue; // Truyền dữ liệu chi tiết cho biểu đồ

            return View();
        }
    }
}