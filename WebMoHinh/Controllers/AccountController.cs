using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebMoHinh.Models;
using WebMoHinh.ViewModels;

namespace WebMoHinh.Controllers
{
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

        // =========================
        // REGISTER
        // =========================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var checkEmail =
                await _userManager.FindByEmailAsync(model.Email);

            if (checkEmail != null)
            {
                ModelState.AddModelError("", "Email đã tồn tại");
                return View(model);
            }

            var checkUser =
                await _userManager.FindByNameAsync(model.UserName);

            if (checkUser != null)
            {
                ModelState.AddModelError("", "Username đã tồn tại");
                return View(model);
            }

            ApplicationUser user = new()
            {
                FullName = model.FullName,
                Age = model.Age,
                Gender = model.Gender,
                Job = model.Job,
                Address = model.Address,
                Email = model.Email,
                UserName = model.UserName
            };

            var result =
                await _userManager.CreateAsync(
                    user,
                    model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(
                    user,
                    model.Role);

                await _signInManager.SignInAsync(
                    user,
                    false);

                TempData["Success"] =
                    "Đăng ký thành công";

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description);
            }

            return View(model);
        }

        // =========================
        // LOGIN
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user =
                await _userManager.FindByNameAsync(
                    model.UserName);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Tài khoản không tồn tại");

                return View(model);
            }

            var result =
                await _signInManager.PasswordSignInAsync(
                    model.UserName,
                    model.Password,
                    false,
                    false);

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "Đăng nhập thành công";

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            ModelState.AddModelError(
                "",
                "Sai tài khoản hoặc mật khẩu");

            return View(model);
        }

        // =========================
        // LOGOUT
        // =========================

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            TempData["Success"] =
                "Đăng xuất thành công";

            return RedirectToAction(
                "Login",
                "Account");
        }

        // =========================
        // ACCESS DENIED
        // =========================

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}