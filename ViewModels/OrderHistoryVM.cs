using System;
using System.Collections.Generic;
using System.Linq;

namespace WebBanPhanMem.ViewModels
{
    public class OrderHistoryVM
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string PaymentStatus { get; set; } = "Pending";

        public List<OrderItemVM> Items { get; set; } = new();

        // Tổng số lượng sản phẩm trong đơn
        public int TotalItems => Items.Sum(i => i.Quantity);

        // Badge hiển thị trạng thái
        public string StatusBadgeClass => Status switch
        {
            "Pending" => "bg-warning text-dark",
            "Processing" => "bg-info text-white",
            "Completed" => "bg-success text-white",
            "Cancelled" => "bg-danger text-white",
            _ => "bg-secondary"
        };

        public string PaymentBadgeClass => PaymentStatus switch
        {
            "Pending" => "bg-warning text-dark",
            "Paid" => "bg-success text-white",
            "Failed" => "bg-danger text-white",
            _ => "bg-secondary"
        };

        // Method để tính tổng số lượng (có thể dùng trong View)
        public int GetTotalQuantity() => TotalItems;
    }

    public class OrderItemVM
    {
        public int Id { get; set; } // Thêm Id để dùng cho DownloadLicense
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;

        public string? LicenseKey { get; set; }
        public bool HasLicense => !string.IsNullOrEmpty(LicenseKey);
    }
}
