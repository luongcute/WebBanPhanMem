using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using WebBanPhanMem.Models;

namespace WebBanPhanMem.Data
{
    public static class SeedData
    {
        public static async Task SeedRolesAndAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            // ensure roles
            string[] roles = new[] { "Admin", "User" };
            foreach (var r in roles)
            {
                if (!await roleManager.RoleExistsAsync(r))
                    await roleManager.CreateAsync(new IdentityRole(r));
            }

            // Admin account from config or default
            var adminEmail = config["SeedAdmin:Email"] ?? "admin@example.com";
            var adminPass = config["SeedAdmin:Password"] ?? "Admin@123";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Administrator" };
                var result = await userManager.CreateAsync(admin, adminPass);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
