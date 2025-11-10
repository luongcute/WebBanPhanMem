namespace WebBanPhanMem.Areas.Admin.Models
{
    public class LicenseKeyAdminViewModel
    {
        public int Id { get; set; }
        public string Key { get; set; } = "";
        public bool IsUsed { get; set; }
        public DateTime? ActivatedDate { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
    }
}
