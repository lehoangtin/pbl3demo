using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StudyShare.ViewModels;
using StudyShare.Services.Interfaces; 
using StudyShare.Services; 
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using StudyShare.Models; // Thêm để dùng AppUser
namespace StudyShare.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<AppUser> _signInManager; // Thêm SignInManager để xử lý đăng xuất

        public AccountController(IAuthService authService, IEmailSender emailSender, SignInManager<AppUser> signInManager)
        {
            _authService = authService;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null) 
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null) 
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // 🔥 KIỂM TRA SỐ LẦN VI PHẠM TRƯỚC KHI ĐĂNG NHẬP
                // Lưu ý: Nếu trong LoginViewModel của bạn dùng trường UserName thay vì Email để đăng nhập, 
                // thì bạn đổi model.Email thành model.UserName nhé.
                var user = await _authService.GetUserByEmailAsync(model.Email); 
                
                if (user != null && user.WarningCount >= 3)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa vĩnh viễn do vi phạm quy định cộng đồng từ 3 lần trở lên.");
                    return View(model);
                }

                // Nếu an toàn (chưa tới 3 lần) thì cho phép đăng nhập bình thường
                var result = await _authService.LoginAsync(model);
                
                if (result.Succeeded)
                {
                    return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) 
                        ? Redirect(returnUrl) 
                        : RedirectToAction("Index", "Home");
                }
                
                // Kiểm tra khóa tài khoản (Lockout) do nhập sai pass nhiều lần
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa tạm thời do đăng nhập sai nhiều lần. Vui lòng thử lại sau.");
                    return View(model);
                }
                
                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng.");
            }
            return View(model);
        }
                [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.RegisterAsync(model);
                
                if (result.Succeeded)
                {
                    var user = await _authService.GetUserByEmailAsync(model.Email);
                    if (user != null) 
                    {
                        var code = await _authService.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                            
                        await _emailSender.SendEmailAsync(model.Email, "Xác nhận địa chỉ email của bạn", $"Vui lòng xác nhận tài khoản của bạn bằng cách <a href='{callbackUrl}'>bấm vào đây</a>.");
                    }

                    TempData["Success"] = "Đăng ký thành công! Vui lòng kiểm tra email để xác nhận tài khoản.";
                    return RedirectToAction("Login");
                }
                
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            // Đăng xuất xong thì đuổi về trang chủ
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string? userId, string? code)
        {
            if (userId == null || code == null) return RedirectToAction("Index", "Home");

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null) return NotFound($"Không tìm thấy người dùng với ID '{userId}'.");

            var result = await _authService.ConfirmEmailAsync(user, code);
            return result.Succeeded ? View("ConfirmSuccess") : View("ConfirmFail");
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _authService.GetUserByEmailAsync(model.Email);
                if (user == null || !(await _authService.IsEmailConfirmedAsync(user)))
                {
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                var code = await _authService.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { code = code }, protocol: HttpContext.Request.Scheme);

                await _emailSender.SendEmailAsync(model.Email, "Đặt lại mật khẩu", $"Vui lòng đặt lại mật khẩu của bạn bằng cách <a href='{callbackUrl}'>nhấn vào đây</a>.");

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation() => View();

        [HttpGet]
        public IActionResult ResetPassword(string? code = null) 
        {
            return code == null ? BadRequest("Cần có mã Token để đổi mật khẩu.") : View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _authService.GetUserByEmailAsync(model.Email);
            if (user == null) return RedirectToAction(nameof(ResetPasswordConfirmation));

            var result = await _authService.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded) return RedirectToAction(nameof(ResetPasswordConfirmation));

            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            
            return View();
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation() => View();

        [HttpGet]
        public IActionResult ResendEmailConfirmation() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _authService.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email xác nhận đã được gửi đi (nếu email tồn tại trong hệ thống).");
                return View(model);
            }

            var code = await _authService.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                
            await _emailSender.SendEmailAsync(model.Email, "Xác nhận địa chỉ email của bạn", $"Vui lòng xác nhận tài khoản của bạn bằng cách <a href='{callbackUrl}'>nhấn vào đây</a>.");

            ModelState.AddModelError(string.Empty, "Email xác nhận đã được gửi lại thành công.");
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}