using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebMoHinh.Models;

namespace WebMoHinh.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserRoleController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRoleController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();

            List<UserRoleViewModel> model = new();

            foreach (var user in users)
            {
                var roles =
                    await _userManager.GetRolesAsync(user);

                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault()
                });
            }

            ViewBag.Roles =
                _roleManager.Roles
                .Select(r => r.Name)
                .ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(
            string userId,
            string role)
        {
            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            var currentRoles =
                await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(
                user,
                currentRoles);

            await _userManager.AddToRoleAsync(
                user,
                role);

            TempData["Success"] =
                "Cập nhật quyền thành công";

            return RedirectToAction(nameof(Index));
        }
    }
}