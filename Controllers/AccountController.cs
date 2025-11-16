using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using WebBanPhanMem.Models;
using WebBanPhanMem.ViewModels;

namespace WebBanPhanWeb.Controllers
{
    [Authorize(AuthenticationSchemes = "UserScheme")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // -------------------------------------------------------------------
        // ĐĂNG KÝ
        // -------------------------------------------------------------------
        [HttpGet, AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");

                // Đăng nhập theo scheme riêng
                var principal = await _signInManager.CreateUserPrincipalAsync(user);
                await HttpContext.SignInAsync("UserScheme", principal, new AuthenticationProperties { IsPersistent = false });

                return RedirectToLocal(returnUrl);
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // -------------------------------------------------------------------
        // ĐĂNG NHẬP
        // -------------------------------------------------------------------
        [HttpGet, AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // --- Đăng nhập bằng scheme UserScheme ---
                var principal = await _signInManager.CreateUserPrincipalAsync(user);
                await HttpContext.SignInAsync("UserScheme", principal, new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                });

                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        // -------------------------------------------------------------------
        // ĐĂNG XUẤT
        // -------------------------------------------------------------------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("UserScheme"); // Logout theo scheme UserScheme
            return RedirectToAction("Login", "Account");
        }

        // -------------------------------------------------------------------
        // PROFILE
        // -------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(AccessDenied));

            var model = new UserProfileVM
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(AccessDenied));

            user.FullName = model.FullName;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // -------------------------------------------------------------------
        // ĐỔI MẬT KHẨU
        // -------------------------------------------------------------------
        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(AccessDenied));

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await HttpContext.SignOutAsync("UserScheme");
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // -------------------------------------------------------------------
        // KHÁC
        // -------------------------------------------------------------------
        [HttpGet, AllowAnonymous]
        public IActionResult AccessDenied() => View();

        // -------------------------------------------------------------------
        // HELPER
        // -------------------------------------------------------------------
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}
