using Microsoft.AspNetCore.Identity;

namespace WebBanPhanMem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
