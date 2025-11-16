using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using WebBanPhanMem.Models;

namespace WebBanPhanMem.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        // Thông tin khách hàng (User chưa đăng nhập thì UserId = null)
        public string? UserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        // Thời gian tạo đơn
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? PaymentDate { get; set; }

        // Tổng tiền (đã tính tại thời điểm đặt hàng)
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Trạng thái đơn hàng
        [StringLength(20)]
        public string Status { get; set; } = "Pending";
        // Pending, Processing, Completed, Cancelled

        // Trạng thái thanh toán
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending";
        // Pending, Paid, Failed
        public DateTime? UpdatedAt { get; set; }


        // Phương thức thanh toán
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "BankTransfer";

        // Mã giao dịch yêu cầu ghi trong nội dung chuyển khoản
        public string? TransactionCode { get; set; }

        // Navigation
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<LicenseKey> LicenseKeys { get; set; } = new List<LicenseKey>();

        // Đánh dấu đã thanh toán
        public void MarkAsPaid()
        {
            PaymentStatus = "Paid";
            Status = "Processing";
            PaymentDate = DateTime.Now;
        }

        // Tổng số lượng item
        public int GetTotalQuantity()
        {
            return Items?.Sum(i => i.Quantity) ?? 0;
        }
    }
}
