using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;
using WebBanPhanMem.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// 1. DATABASE
// =====================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================================================
// 2. IDENTITY – dùng cho User
// =====================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// =====================================================
// 3. AUTHENTICATION SCHEMES
// =====================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "UserScheme";
    options.DefaultSignInScheme = "UserScheme";
    options.DefaultChallengeScheme = "UserScheme";
})
.AddCookie("UserScheme", options =>
{
    options.Cookie.Name = "UserAuthCookie";
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
})
.AddCookie("AdminAuth", options =>
{
    options.Cookie.Name = "AdminAuthCookie";
    options.LoginPath = "/Admin/Account/Login";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
});

// =====================================================
// 4. ĐĂNG KÝ DỊCH VỤ KHÁC
// =====================================================

// Đăng ký EmailSender
builder.Services.AddScoped<IEmailSender, EmailSender>();
// LicenseService
builder.Services.AddScoped<ILicenseService, LicenseService>();

// DinkToPdf
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

// MVC + Razor + Session
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSession();

var app = builder.Build();

// =====================================================
// 5. MIDDLEWARE
// =====================================================
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

// =====================================================
// 6. ROUTING
// =====================================================

// AREA ADMIN
app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

// ROUTE USER MẶC ĐỊNH
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
