using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebBanPhanMem.Models;
using WebBanPhanMem.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminAuth", Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager,
                                 UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ===========================
        // GET: Admin/Account/Login
        // ===========================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // ===========================
        // POST: Admin/Account/Login
        // ===========================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View(model);
            }

            // Kiểm tra Role ADMIN
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                ModelState.AddModelError("", "Tài khoản này không có quyền truy cập Admin.");
                return View(model);
            }

            // Kiểm tra mật khẩu
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                ModelState.AddModelError("", "Sai mật khẩu.");
                return View(model);
            }

            // Tạo claim
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Đăng nhập bằng AdminAuth SCHEME
            await HttpContext.SignInAsync("AdminAuth", principal, new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            });

            // Redirect nếu có ReturnUrl
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Trang Admin Dashboard
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        // ===========================
        // POST: Admin/Account/Logout
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminAuth");
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }
    }
}
