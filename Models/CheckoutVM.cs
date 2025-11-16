using System.ComponentModel.DataAnnotations;
using WebBanPhanMem.Models;
using System.Collections.Generic;

namespace WebBanPhanMem.Models.ViewModels
{
    public class CheckoutVM
    {
        // Thông tin người dùng
        [Required(ErrorMessage = "Vui lòng nhập Tên người nhận.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Chi tiết giỏ hàng
        public List<OrderItem> CartItems { get; set; } = new List<OrderItem>();

        public decimal TotalAmount { get; set; }

        // Thanh toán
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public string PaymentMethod { get; set; } = "BankTransfer";

        // Người dùng đã đăng nhập
        public string? UserId { get; set; }

        // Mã đơn hàng sinh sau khi đặt
        public string? TransactionCode { get; set; }
    }
}
