using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebBanPhanMem.Areas.Admin.Models
{
    public class ProductAdminViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = "";

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá không hợp lệ")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Display(Name = "Hình ảnh hiện tại")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Chọn hình mới")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        [Display(Name = "Số lượng license còn lại")]
        public int LicenseCount { get; set; }

        public bool HasLicenseKey { get; set; } // true nếu sản phẩm dùng key

    }
}
