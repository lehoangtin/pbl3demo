using Microsoft.AspNetCore.Mvc;

namespace StudyShare.Controllers // Đảm bảo namespace đồng nhất
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}