using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Models;

namespace WebBanPhanMem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<LicenseKey> LicenseKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình độ chính xác cho các trường decimal
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);
        }
    }
}
