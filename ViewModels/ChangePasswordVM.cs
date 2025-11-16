using System.ComponentModel.DataAnnotations;

namespace WebBanPhanMem.ViewModels
{
    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;


        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

    }
}