using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.DTOs.Requests;
using StudyShare.Services.Interfaces;
using System.Security.Claims; // Bổ sung để lấy User ID

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        // Xóa hoàn toàn UserManager
        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            // Tối ưu: Lấy từ Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");
            
            var request = await _userService.GetProfileForEditAsync(userId);
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileUpdateRequest request)
        {
            if (!ModelState.IsValid) return View("Index", request);
            
            // Tối ưu: Lấy từ Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");
            
            request.Id = userId; 
            await _userService.UpdateProfileAsync(request);
            
            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Index");
        }
    }
}