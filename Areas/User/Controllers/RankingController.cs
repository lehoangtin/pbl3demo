using Microsoft.AspNetCore.Mvc;
using StudyShare.Services.Interfaces;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    public class RankingController : Controller
    {
        private readonly IUserService _userService;

        public RankingController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var topUsers = await _userService.GetTopRankingAsync(10); // Lấy top 10
            return View(topUsers);
        }
    }
}
