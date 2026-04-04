using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using StudyShare.Models;
using StudyShare.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace StudyShare.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly EmailSender _emailSender;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            EmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // ================= REGISTER =================
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new AppUser
            {
                Email = model.Email,
                UserName = model.Email,
                FullName = model.FullName
            };

            await _userManager.CreateAsync(user, model.Password);

            // mặc định user là "User"
            await _userManager.AddToRoleAsync(user, "User");
            return RedirectToAction("Login");
        }
        // ================= CONFIRM EMAIL =================

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return Content("Link không hợp lệ");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Content("User không tồn tại");

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
                return Content("✅ Xác nhận email thành công!");

            return Content("❌ Xác nhận thất bại!");
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) 
        {
            ModelState.AddModelError("", "Sai email hoặc password");
            return View(model);
        }

    // 🔥 KIỂM TRA XEM CÓ BỊ BAN KHÔNG
        if (user.IsBanned)
        {
        ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa bởi Admin.");
        return View(model);
        }

            // 🔥 check confirm email
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("", "Bạn cần xác nhận email trước!");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                false,
                false
            );

           // File: Controllers/AccountController.cs
            if (result.Succeeded)
            {
                // Kiểm tra quyền để chuyển hướng đúng Area
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                return RedirectToAction("Index", "Home"); // Trang chủ chung
            }
            
            ModelState.AddModelError("", "Sai email hoặc password");
            return View(model);
        }

        // ================= LOGOUT =================

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ================= CHANGE PASSWORD =================

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.OldPassword,
                model.NewPassword
            );

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Profile", "User", new { area = "User", id = user.Id });
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return RedirectToAction("ForgotPasswordConfirmation");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var link = Url.Action(
                "ResetPassword",
                "Account",
                new { email = user.Email, token = token },
                Request.Scheme
            );

            await _emailSender.SendEmailAsync(
                user.Email,
                "Reset mật khẩu",
                $"Click: <a href='{link}'>Reset Password</a>"
            );

            return RedirectToAction("ForgotPasswordConfirmation");
        }
        public IActionResult ForgotPasswordConfirmation()
        {
            return Content("📧 Đã gửi email reset password!");
        }
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return Content("Link không hợp lệ");

            return View(new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            });
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return RedirectToAction("ResetPasswordConfirmation");

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.NewPassword
            );

            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation");

            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);

            return View(model);
        }
        public IActionResult ResetPasswordConfirmation()
        {
            return Content("✅ Reset password thành công!");
        }

    }
}