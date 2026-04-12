using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using StudyShare.Models;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    public class RankingController : Controller
    {
        private readonly AppDbContext _context;

        public RankingController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy Top 10 người dùng có điểm cao nhất
            var topUsers = await _context.Users
                .OrderByDescending(u => u.Points)
                .Take(10)
                .ToListAsync();

            return View(topUsers);
        }
    }
}