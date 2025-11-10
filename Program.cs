using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;
using WebBanPhanMem.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Kết nối CSDL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity (quản lý người dùng)
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 3. Đăng ký các Service (DI)
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();

// 4. Session
builder.Services.AddSession();

// 5. MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- BẮT ĐẦU PHẦN THÊM VÀO ---
// Tự động Migrate Database khi khởi động
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Áp dụng các Migration đang chờ xử lý
        context.Database.Migrate();

        // Tùy chọn: Thêm Seed Data nếu bạn có hàm SeedData.Initialize
        // SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration or seeding.");
        // Bạn có thể thêm dòng này nếu muốn ứng dụng sập khi lỗi DB, 
        // nhưng hiện tại để nó chạy tiếp để kiểm tra lỗi khác.
        // throw; 
    }
}
// --- KẾT THÚC PHẦN THÊM VÀO ---


// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Khu vực (Areas)
app.MapControllerRoute(
    name: "areas",
    // CHỈNH SỬA: Đặt Dashboard làm Controller mặc định cho Area Admin
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

// Tuyến mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();