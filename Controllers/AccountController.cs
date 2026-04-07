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
                Email = model.Email.Trim(), 
                UserName = model.Email.Trim(), 
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
                    ModelState.AddModelError("", "Tài khoản đã tạo nhưng hệ thống mail đang nghẽn. Bạn có thể đăng nhập để thử 'Gửi lại Email xác nhận'.");
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
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                
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

    // FIX: Thêm .Trim() để loại bỏ khoảng trắng thừa
    var user = await _userManager.FindByEmailAsync(model.Email.Trim());
    
    if (user != null)
    {
        if (user.IsBanned)
        {
            ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
            return View(model);
        }

        // QUAN TRỌNG: Kiểm tra EmailConfirmed
        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            ModelState.AddModelError("", "Bạn cần xác nhận Email trước khi đăng nhập.");
            return View(model);
        }

        // Truyền chính đối tượng user vào để Identity xử lý tốt nhất
        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            
            return RedirectToAction("Index", "Home");
        }
    }

    // Nếu chạy đến đây là thất bại
    ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
    return View(model);
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
            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user))) return View("ForgotPasswordConfirmation");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // FIX 3: Dùng WebEncoders để Token không bị lỗi ký tự đặc biệt khi gửi mail
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var callbackUrl = Url.Action("ResetPassword", "Account", new { token = token, email = user.Email }, Request.Scheme);
            await _emailSender.SendEmailAsync(model.Email, "Đặt lại mật khẩu", $"Click vào đây: <a href='{callbackUrl}'>Đặt lại mật khẩu</a>");
            return View("ForgotPasswordConfirmation");
        }
        // ================= ĐẶT LẠI MẬT KHẨU =================
[AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null) return BadRequest("Liên kết hỏng");
            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

// 2. ĐẶT LẠI MẬT KHẨU (Lúc xử lý lưu)
[AllowAnonymous]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
{
if (!ModelState.IsValid) return View(model);

    // Kiểm tra model.Email có null không trước khi dùng (Fix CS8604)
    if (string.IsNullOrEmpty(model.Email))
    {
        ModelState.AddModelError("", "Email không được để trống.");
        return View(model);
    }

    var user = await _userManager.FindByEmailAsync(model.Email.Trim());
    
    if (user == null)
    {
        // KHÔNG RedirectToAction ở đây! Phải báo lỗi để người dùng biết
        ModelState.AddModelError("", "Không tìm thấy người dùng ứng với Email này.");
        return View(model);
    }
    // Giải mã token (đảm bảo khớp với lúc bạn gửi mail)
    try {
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

        if (result.Succeeded)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        // Hiển thị lỗi từ Identity (VD: Mật khẩu quá yếu)
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
    }
    catch {
        ModelState.AddModelError("", "Mã xác thực không hợp lệ.");
    }
    
    return View(model);
}
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();
    }
}