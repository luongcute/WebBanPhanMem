// File: Models/OrderItem.cs (ĐÃ HOÀN CHỈNH)

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanPhanMem.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        // Khóa ngoại
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!; // Đảm bảo Product Navigation Property tồn tại

        // Thông tin cơ bản
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // License key
        public bool RequiresLicenseKey { get; set; }
        public string? LicenseKey { get; set; }
        public DateTime? LicenseAssignedAt { get; set; }

        // 💡 KHẮC PHỤC LỖI CS1061: Total
        [NotMapped] // Đánh dấu là không phải cột trong DB
        public decimal Total => Price * Quantity;

        // Phương thức đơn giản
        public void AssignLicense(string licenseKey)
        {
            LicenseKey = licenseKey;
            LicenseAssignedAt = DateTime.Now;
            RequiresLicenseKey = false;
        }

        public bool HasLicenseAssigned()
        {
            return !string.IsNullOrEmpty(LicenseKey);
        }
    }
}