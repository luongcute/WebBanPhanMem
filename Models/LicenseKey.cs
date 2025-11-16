using System.ComponentModel.DataAnnotations.Schema;
using WebBanPhanMem.Models;

public class LicenseKey
{
    public int Id { get; set; }
    public string KeyContent { get; set; } = ""; // Tên Key thực tế
    public int ProductId { get; set; }

    // Status: Thay vì dùng IsUsed (đã dùng/chưa dùng), dùng Status rộng hơn (Available/Sold/Used)
    // Nhưng nếu bạn muốn giữ IsUsed, chúng ta sẽ sử dụng IsUsed để thay thế trạng thái Sold
    public bool IsUsed { get; set; } = false;

    public DateTime? ActivatedDate { get; set; }

    // Khóa ngoại đến Đơn hàng (để biết Key được bán qua đơn hàng nào)
    public int? OrderId { get; set; }

    // Liên kết ngược
    public Product? Product { get; set; }

    // Liên kết ngược đến Order
    public Order? Order { get; set; }
}