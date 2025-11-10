namespace WebBanPhanMem.Models
{
    public class LicenseKey
    {
        public int Id { get; set; }
        public string Key { get; set; } = "";
        public int ProductId { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? ActivatedDate { get; set; }
    }
}
