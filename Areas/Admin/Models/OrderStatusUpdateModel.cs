namespace WebBanPhanMem.Models.ViewModels
{
    public class OrderStatusUpdateModel
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
