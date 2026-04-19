using Microsoft.AspNetCore.Mvc;
using StudyShare.ViewModels;
using StudyShare.Services.Interfaces;
using StudyShare.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using StudyShare.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; 
namespace StudyShare.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly EmailSender _emailSender; // Vẫn giữ lại để Controller lo việc gửi mail URL

        // Không tiêm UserManager / SignInManager ở đây nữa
        public AccountController(IAuthService authService, EmailSender emailSender)
        {
            _authService = authService;
            _emailSender = emailSender;
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
                var result = await _authService.LoginAsync(model); // Gọi qua Service
                
                if (result.Succeeded)
                {
                    return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) 
                        ? Redirect(returnUrl) 
                        : RedirectToAction("Index", "Home");
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
                var result = await _authService.RegisterAsync(model); // Gọi qua Service
                
                if (result.Succeeded)
                {
                    var user = await _authService.GetUserByEmailAsync(model.Email);
                    var code = await _authService.GenerateEmailConfirmationTokenAsync(user);
                    
                    // Controller chỉ lo tạo link và gửi mail
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    await _emailSender.SendEmailAsync(model.Email, "Xác nhận tài khoản", $"Vui lòng click vào <a href='{callbackUrl}'>đây</a> để xác nhận.");

                    TempData["Success"] = "Đăng ký thành công! Vui lòng kiểm tra email.";
                    return RedirectToAction("Login");
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }
        
        // Các Action khác (ForgotPassword, ResetPassword) bạn cũng sửa tương tự, chỉ việc gọi các hàm tương ứng từ _authService!
    }
}