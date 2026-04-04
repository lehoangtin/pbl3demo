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
            // Lấy ID người dùng hiện tại đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Chuyển hướng sang Action Profile của UserController để xử lý dữ liệu
            return RedirectToAction("Profile", "User", new { area = "User", id = userId });
        }
    }
}