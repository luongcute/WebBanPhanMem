using System;
using System.Collections.Generic;

namespace WebBanPhanMem.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        // Tổng quan
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        // License key
        public int TotalLicenseKeys { get; set; }
        public int UsedLicenseKeys { get; set; }

        // Doanh thu theo tháng (để vẽ biểu đồ)
        public List<string> RevenueMonths { get; set; } = new();
        public List<decimal> RevenueValues { get; set; } = new();

        // Top sản phẩm bán chạy
        public List<TopProductViewModel> TopProducts { get; set; } = new();

        // Đơn hàng gần đây
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new();
    }

    public class TopProductViewModel
    {
        public string ProductName { get; set; } = "";
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RecentOrderViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
    }
}