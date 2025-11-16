using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using CsvHelper;
using CsvHelper.Configuration;
using WebBanPhanMem.Data;
using WebBanPhanMem.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminAuth", Roles = "Admin")]

    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor để inject DbContext
        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action cơ bản để hiển thị trang báo cáo (nếu có)
        public IActionResult Index()
        {
            return View();
        }

        // --- 1. Sự kiện xuất CSV ---
        public async Task<IActionResult> ExportCsv(string startDate, string endDate)
        {
            // 1.1. Lấy dữ liệu 
            var reportData = await GetDetailedReportDataAsync(startDate, endDate);

            // 1.2. Tạo nội dung CSV dưới dạng byte array (đã bao gồm BOM)
            var csvBytes = GenerateCsvContent(reportData); // Đã đổi từ csvContent sang csvBytes

            // 1.3. Trả về file (không cần Encoding.UTF8.GetBytes nữa)
            return File(
                csvBytes, // Gửi trực tiếp byte array có BOM
                "text/csv",
                $"BaoCaoDoanhThu_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            );
        }

        // --- 2. Sự kiện xuất PDF ---
        public async Task<IActionResult> ExportPdf(string startDate, string endDate)
        {
            // 2.1. Lấy dữ liệu
            var reportData = await GetDetailedReportDataAsync(startDate, endDate);

            // 2.2. Tạo nội dung PDF (Yêu cầu thư viện bên ngoài)
            // Cần triển khai GeneratePdfContent(reportData) và sử dụng thư viện như PdfSharp

            // Hiện tại tạm trả về nội dung trống để tránh lỗi build
            return Content("Chức năng xuất PDF chưa được cấu hình thư viện.");
        }

        // --- HÀM HỖ TRỢ LẤY DỮ LIỆU BÁO CÁO ---
        private async Task<List<ReportRecord>> GetDetailedReportDataAsync(string startDate, string endDate)
        {
            DateTime start = DateTime.MinValue;
            DateTime end = DateTime.MaxValue;

            // Phân tích cú pháp ngày bắt đầu
            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out DateTime s))
            {
                start = s.Date;
            }
            // Phân tích cú pháp ngày kết thúc
            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out DateTime e))
            {
                end = e.Date.AddDays(1).AddTicks(-1); // Bao gồm cả ngày kết thúc
            }

            const string PAID_STATUS = "paid";

            // Truy vấn Database BẤT ĐỒNG BỘ và ánh xạ sang ReportRecord
            var reportData = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Where(o => (o.PaymentStatus ?? "").ToLower() == PAID_STATUS &&
                            o.CreatedAt >= start && o.CreatedAt <= end)
                .Select(o => new ReportRecord
                {
                    OrderId = o.Id,
                    OrderDate = o.CreatedAt,
                    CustomerName = (o.ApplicationUser != null ? o.ApplicationUser.FullName : o.CustomerName) ?? "Khách vãng lai",
                    TotalAmount = o.TotalAmount,
                    Status = o.PaymentStatus,
                    TotalItems = o.Items.Sum(oi => (int?)oi.Quantity) ?? 0
                })
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return reportData;
        }

        // --- HÀM HỖ TRỢ TẠO NỘI DUNG CSV ---
        // ✅ THAY ĐỔI: Hàm này giờ trả về byte[] để đảm bảo BOM được thêm vào.
        private byte[] GenerateCsvContent(List<ReportRecord> data)
        {
            // TẠO ENCODING: Sử dụng UTF8Encoding(true) để BẮT BUỘC thêm BOM vào đầu file.
            var encodingWithBOM = new UTF8Encoding(true);

            using (var memoryStream = new MemoryStream())
            // Sử dụng StreamWriter với encoding có BOM
            using (var writer = new StreamWriter(memoryStream, encodingWithBOM, bufferSize: 1024, leaveOpen: true))
            // KHUYẾN NGHỊ: Sử dụng CultureInfo("vi-VN") để định dạng dấu phẩy/chấm thập phân (nếu cần)
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Đăng ký mapping để có tiêu đề tiếng Việt và định dạng
                csv.Context.RegisterClassMap(typeof(ReportRecordMap));

                // Ghi dữ liệu
                csv.WriteRecords(data);

                writer.Flush(); // Đảm bảo mọi thứ đã được ghi vào MemoryStream

                return memoryStream.ToArray(); // Trả về byte array (có BOM ở đầu)
            }
        }
    }
}