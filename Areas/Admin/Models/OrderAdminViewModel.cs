namespace WebBanPhanMem.Areas.Admin.Models
{
    public class OrderAdminViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public string Status { get; set; } = "pending";
        public string PaymentStatus { get; set; } = "Pending";
        public DateTime? PaymentDate { get; set; }
    }
}