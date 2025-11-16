using System.ComponentModel.DataAnnotations;

namespace WebBanPhanMem.Models
{
    public class ContactMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Nội dung")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Ngày gửi")]
        public DateTime SentDate { get; set; } = DateTime.Now;

        [Display(Name = "Đã đọc")]
        public bool IsRead { get; set; } = false;
    }
}