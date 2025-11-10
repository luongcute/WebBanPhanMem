namespace WebBanPhanMem.Areas.Admin.Models
{
    public class OrderAdminViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
    }
}
