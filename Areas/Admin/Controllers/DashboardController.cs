using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Data;
using WebBanPhanMem.Areas.Admin.Models;
using WebBanPhanMem.Models;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebBanPhanMem.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminAuth", Roles = "Admin")]

    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConverter _converter;

        public DashboardController(ApplicationDbContext context, IConverter converter)
        {
            _context = context;
            _converter = converter;
        }

        public async Task<IActionResult> Index()
        {
            const string PAID_STATUS = "paid";
            const int MONTHS_TO_SHOW = 6;
            var viewModel = new DashboardViewModel();

            viewModel.TotalProducts = await _context.Products.CountAsync();
            viewModel.TotalCategories = await _context.Categories.CountAsync();
            viewModel.TotalOrders = await _context.Orders.CountAsync();
            viewModel.TotalUsers = await _context.Users.CountAsync();

            viewModel.TotalRevenue = await _context.Orders
                .Where(o => (o.PaymentStatus ?? "").ToLower() == PAID_STATUS)
                .SelectMany(o => o.Items)
                .SumAsync(i => (decimal?)(i.Price * i.Quantity)) ?? 0m;

            viewModel.TotalLicenseKeys = await _context.LicenseKeys.CountAsync();
            viewModel.UsedLicenseKeys = await _context.LicenseKeys.CountAsync(lk => lk.IsUsed);

            var ordersPaidQuery = _context.Orders
                .Where(o => (o.PaymentStatus ?? "").ToLower() == PAID_STATUS)
                .AsQueryable();

            var monthlyRevenueRaw = await ordersPaidQuery
                .Where(o => o.PaymentDate.HasValue)
                .GroupBy(o => new { o.PaymentDate!.Value.Year, o.PaymentDate.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Take(MONTHS_TO_SHOW)
                .ToListAsync();

            var monthlyRevenue = monthlyRevenueRaw
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .Select(x => new
                {
                    Month = $"{x.Month}/{x.Year}",
                    Total = x.Total
                })
                .ToList();

            viewModel.RevenueMonths = monthlyRevenue.Select(x => x.Month).ToList();
            viewModel.RevenueValues = monthlyRevenue.Select(x => x.Total).ToList();

            viewModel.TopProducts = await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => ordersPaidQuery.Any(o => o.Id == oi.OrderId))
                .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
                .Select(g => new TopProductViewModel
                {
                    ProductName = g.Key.Name!,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Price * oi.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToListAsync();

            viewModel.RecentOrders = await _context.Orders
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new RecentOrderViewModel
                {
                    OrderId = o.Id,
                    CustomerName = (o.ApplicationUser != null ? o.ApplicationUser.FullName : o.CustomerName) ?? "Khách vãng lai",
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.TotalAmount
                })
                .ToListAsync();

            return View(viewModel);
        }

        // Action xuất PDF
        public async Task<IActionResult> ExportPdf()
        {
            const string PAID_STATUS = "paid";
            const int MONTHS_TO_SHOW = 6;

            var viewModel = new DashboardViewModel();

            // Load dữ liệu tương tự Index
            viewModel.TotalProducts = await _context.Products.CountAsync();
            viewModel.TotalCategories = await _context.Categories.CountAsync();
            viewModel.TotalOrders = await _context.Orders.CountAsync();
            viewModel.TotalUsers = await _context.Users.CountAsync();

            viewModel.TotalRevenue = await _context.Orders
                .Where(o => (o.PaymentStatus ?? "").ToLower() == PAID_STATUS)
                .SelectMany(o => o.Items)
                .SumAsync(i => (decimal?)(i.Price * i.Quantity)) ?? 0m;

            viewModel.TotalLicenseKeys = await _context.LicenseKeys.CountAsync();
            viewModel.UsedLicenseKeys = await _context.LicenseKeys.CountAsync(lk => lk.IsUsed);

            var ordersPaidQuery = _context.Orders
                .Where(o => (o.PaymentStatus ?? "").ToLower() == PAID_STATUS)
                .AsQueryable();

            var monthlyRevenueRaw = await ordersPaidQuery
                .Where(o => o.PaymentDate.HasValue)
                .GroupBy(o => new { o.PaymentDate!.Value.Year, o.PaymentDate.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Take(MONTHS_TO_SHOW)
                .ToListAsync();

            var monthlyRevenue = monthlyRevenueRaw
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .Select(x => new
                {
                    Month = $"{x.Month}/{x.Year}",
                    Total = x.Total
                })
                .ToList();

            viewModel.RevenueMonths = monthlyRevenue.Select(x => x.Month).ToList();
            viewModel.RevenueValues = monthlyRevenue.Select(x => x.Total).ToList();

            viewModel.TopProducts = await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => ordersPaidQuery.Any(o => o.Id == oi.OrderId))
                .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
                .Select(g => new TopProductViewModel
                {
                    ProductName = g.Key.Name!,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Price * oi.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToListAsync();

            viewModel.RecentOrders = await _context.Orders
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new RecentOrderViewModel
                {
                    OrderId = o.Id,
                    CustomerName = (o.ApplicationUser != null ? o.ApplicationUser.FullName : o.CustomerName) ?? "Khách vãng lai",
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.TotalAmount
                })
                .ToListAsync();

            // Render View PdfReport.cshtml thành chuỗi HTML
            var htmlContent = await this.RenderViewAsync("PdfReport", viewModel);

            // Cấu hình PDF
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                    DPI = 130,
                },
                Objects = {
                    new ObjectSettings() {
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8" },
                    }
                }
            };

            var pdfBytes = _converter.Convert(doc);

            return File(pdfBytes, "application/pdf", "DashboardReport.pdf");


        }
    }
}
