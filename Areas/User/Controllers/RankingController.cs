using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using Microsoft.AspNetCore.Identity; // Yêu cầu thêm thư viện này
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Yêu cầu thêm thư viện này

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize] // Cả User và Admin đều có thể xem bảng xếp hạng
    public class RankingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager; // Khai báo thêm UserManager

        // Tiêm cả AppDbContext và UserManager vào constructor
        public RankingController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Chỉ lấy những tài khoản có Role là "User" (tự động loại bỏ Admin)
            var normalUsers = await _userManager.GetUsersInRoleAsync("User");
            
            // 2. Sắp xếp điểm số giảm dần và lấy ra Top 10
            var topUsers = normalUsers
                .OrderByDescending(u => u.Points)
                .Take(10)
                .ToList();

            // 3. Trả danh sách về cho View
            return View(topUsers);
        }
    }
}