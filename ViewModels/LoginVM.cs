using System.ComponentModel.DataAnnotations;

namespace WebBanPhanMem.ViewModels
{
    public class LoginVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // Giá trị mặc định

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty; // Giá trị mặc định

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; } = false; // Có thể giữ default false
    }
}
