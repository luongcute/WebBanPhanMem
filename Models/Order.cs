using System.ComponentModel.DataAnnotations;

namespace WebBanPhanMem.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<OrderItem>? Items { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
