using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // Thêm dòng này để dùng ClaimTypes

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class ProfileController : Controller
    {
        // Sử dụng lại logic của UserController để lấy dữ liệu hoặc chuyển hướng
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            return RedirectToAction("Profile", "User", new { area = "User", id = userId });
        }
    }
}