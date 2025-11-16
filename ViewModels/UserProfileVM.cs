using System.ComponentModel.DataAnnotations;

namespace WebBanPhanMem.ViewModels
{
    public class UserProfileVM
    {
        // Thuộc tính để hiển thị/chỉnh sửa tên đầy đủ của khách hàng
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ và Tên")]
        public string FullName { get; set; } = string.Empty;


        // Thuộc tính chỉ hiển thị (thường là email/username, không cho phép sửa trực tiếp)
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ.")]
        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; } = string.Empty;


        // Bạn có thể thêm các thuộc tính khác của ApplicationUser ở đây
        // Ví dụ: Số điện thoại, địa chỉ, ngày sinh, v.v.

        /*
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? PhoneNumber { get; set; } 
        */
    }
}