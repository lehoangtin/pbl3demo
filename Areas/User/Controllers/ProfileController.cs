using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize] // user đăng nhập mới vào
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}