using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using StudyShare.Models;
using Microsoft.AspNetCore.Authorization; // Thêm dòng này

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize] // Thêm dòng này: Bắt buộc đăng nhập mới xem được Bảng xếp hạng
    public class RankingController : Controller
    {
        private readonly AppDbContext _context;

        public RankingController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var topUsers = await _context.Users
                .OrderByDescending(u => u.Points)
                .Take(10)
                .ToListAsync();

            return View(topUsers);
        }
    }
}