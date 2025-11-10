using System.ComponentModel.DataAnnotations;

namespace WebBanPhanMem.Areas.Admin.Models
{
    public class CategoryAdminViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = "";

        [Display(Name = "Số lượng sản phẩm")]
        public int ProductCount { get; set; }
    }
}
