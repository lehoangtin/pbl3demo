using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // CẦN THÊM DÒNG NÀY

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager; // Khai báo thêm UserManager

        // Tiêm UserManager vào Constructor
        public HomeController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Thống kê người dùng
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.BannedUsers = await _context.Users.CountAsync(u => u.IsBanned);
            
            // 2. Thống kê tài liệu
            ViewBag.TotalDocuments = await _context.Documents.CountAsync();
            ViewBag.ApprovedDocuments = await _context.Documents.CountAsync(d => d.IsApproved);
            ViewBag.PendingDocuments = await _context.Documents.CountAsync(d => !d.IsApproved);
            
            // 3. Thống kê hỏi đáp & danh mục
            ViewBag.TotalQuestions = await _context.Questions.CountAsync();
            ViewBag.TotalAnswers = await _context.Answers.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();

            // 4. Lấy danh sách Top 5 người dùng điểm cao nhất (🔥 ĐÃ SỬA: CHỈ LẤY ROLE "USER")
            var normalUsers = await _userManager.GetUsersInRoleAsync("User");
            ViewBag.TopUsers = normalUsers
                .OrderByDescending(u => u.Points)
                .Take(5)
                .ToList();

            // 5. Lấy 5 tài liệu mới nhất đang chờ duyệt
            ViewBag.RecentPendingDocs = await _context.Documents
                .Include(d => d.User)
                .Where(d => !d.IsApproved)
                .OrderByDescending(d => d.UploadDate)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}