using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.Services.Interfaces;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(IUserService userService, UserManager<AppUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var request = await _userService.GetProfileForEditAsync(userId);
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileUpdateRequest request)
        {
            if (!ModelState.IsValid) return View("Index", request);
            
            // Chống hack: Ép ID bằng đúng ID người đang đăng nhập
            request.Id = _userManager.GetUserId(User); 
            await _userService.UpdateProfileAsync(request);
            
            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Index");
        }
    }
}
