using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Areas.Admin.Models;
using WebBanPhanMem.Models;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminAuth", Roles = "Admin")]

    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // ⭐ BẮT BUỘC PHẢI CÓ CONSTRUCTOR NÀY
        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var vm = new List<AdminUserVM>();

            foreach (var u in allUsers)
            {
                if (await _userManager.IsInRoleAsync(u, "Admin"))
                    continue;

                var roles = await _userManager.GetRolesAsync(u);

                vm.Add(new AdminUserVM
                {
                    Id = u.Id,
                    FullName = u.FullName!,
                    Email = u.Email!,
                    Role = roles.FirstOrDefault() ?? "User",
                    IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.Now
                });
            }

            return View(vm);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var vm = new AdminUserVM
            {
                Id = user.Id,
                FullName = user.FullName!,
                Email = user.Email!,
                Role = roles.FirstOrDefault() ?? "User"
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdminUserVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;

            await _userManager.UpdateAsync(user);

            var oldRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, oldRoles);
            await _userManager.AddToRoleAsync(user, model.Role);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return RedirectToAction("Index");
        }
    }
}
