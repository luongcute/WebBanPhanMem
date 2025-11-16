using System.ComponentModel.DataAnnotations;

namespace WebBanPhanMem.Areas.Admin.Models
{
    public class AdminUserVM
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Họ và Tên")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Vai trò")]
        public string Role { get; set; } = string.Empty;

        public bool IsLocked { get; set; }
    }
}
