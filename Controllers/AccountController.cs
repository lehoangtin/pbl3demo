using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using StudyShare.Models;
using StudyShare.Services;
using Microsoft.AspNetCore.Authorization;
using StudyShare.ViewModels;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

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

        // ================= ĐĂNG KÝ =================
        [AllowAnonymous]
        public IActionResult Register() => View();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new AppUser { 
                Email = model.Email, 
                UserName = model.Email, 
                FullName = model.FullName,
                EmailConfirmed = false 
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                
                var confirmationLink = Url.Action("ConfirmEmail", "Account", 
                    new { userId = user.Id, token = token }, Request.Scheme);
                
                try {
                    await _emailSender.SendEmailAsync(user.Email, "Xác nhận tài khoản StudyShare", 
                        $"Chào {user.FullName}, vui lòng click vào link để kích hoạt tài khoản: <a href='{confirmationLink}'>Kích hoạt ngay</a>");
                } catch {
                    // Nếu lỗi gửi mail, vẫn tạo user nhưng thông báo lỗi hệ thống mail
                    ModelState.AddModelError("", "Tài khoản đã tạo nhưng lỗi gửi mail xác nhận. Vui lòng liên hệ Admin.");
                    return View(model);
                }

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng kiểm tra Email để kích hoạt tài khoản.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        // ================= XÁC NHẬN EMAIL =================
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null) return View("ConfirmFail");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return View("ConfirmFail");

            try {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
                var result = await _userManager.ConfirmEmailAsync(user, token);
                
                if (result.Succeeded) {
                    TempData["SuccessMessage"] = "Xác nhận Email thành công. Bạn có thể đăng nhập.";
                    return View("ConfirmSuccess");
                }
            } catch { }

            return View("ConfirmFail");
        }

        // ================= ĐĂNG NHẬP =================
        [AllowAnonymous]
        public IActionResult Login() => View();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // Kiểm tra tài khoản bị khóa
                if (user.IsBanned)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa do vi phạm chính sách.");
                    return View(model);
                }

                // Kiểm tra xác nhận Email
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError("", "Bạn cần xác nhận Email trước khi đăng nhập.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        // ================= ĐĂNG XUẤT =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        // ================= QUÊN MẬT KHẨU =================
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Không báo user không tồn tại để bảo mật, chỉ báo kiểm tra mail
                return View("ForgotPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var callbackUrl = Url.Action("ResetPassword", "Account", 
                new { token, email = user.Email }, Request.Scheme);

            await _emailSender.SendEmailAsync(model.Email, "Đặt lại mật khẩu StudyShare",
                $"Vui lòng click vào đây để đặt lại mật khẩu: <a href='{callbackUrl}'>Đặt lại mật khẩu</a>");

            return View("ForgotPasswordConfirmation");
        }

        // ================= ĐẶT LẠI MẬT KHẨU =================
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null) return BadRequest("Liên kết không hợp lệ");
            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction(nameof(ResetPasswordConfirmation));

            try 
            {
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

                if (result.Succeeded) return RedirectToAction(nameof(ResetPasswordConfirmation));

                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            }
            catch 
            {
                ModelState.AddModelError("", "Mã xác thực không hợp lệ hoặc đã hết hạn.");
            }
            
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation() => View();

        // ================= TỪ CHỐI TRUY CẬP =================
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}