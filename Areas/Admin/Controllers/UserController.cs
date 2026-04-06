using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudyShare.ViewModels;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public UserController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _userManager.Users.ToListAsync());
        }

        // --- ACTION CHI TIẾT NGƯỜI DÙNG ---
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // 1. Lấy thông tin User (Đây là cái View đang đợi)
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // 2. Lấy danh sách Vai trò
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles.Any() ? string.Join(", ", roles) : "Thành viên";

            // 3. Thống kê số lượng (Hiện ở các ô trên đầu trang)
            ViewBag.DocumentCount = await _context.Documents.CountAsync(d => d.UserId == id);
            ViewBag.QuestionCount = await _context.Questions.CountAsync(q => q.UserId == id);
            ViewBag.AnswerCount = await _context.Answers.CountAsync(a => a.UserId == id);

            // 4. Lấy dữ liệu liên quan và bỏ vào ViewBag (Để View gọi ra sau)
            ViewBag.RecentDocuments = await _context.Documents
                .Where(d => d.UserId == id)
                .OrderByDescending(d => d.UploadDate) // Lưu ý: UploadDate hoặc CreatedAt tùy DB của bạn
                .Take(5)
                .ToListAsync() ?? new List<Document>();

            ViewBag.RecentQuestions = await _context.Questions
                .Where(q => q.UserId == id)
                .OrderByDescending(q => q.CreatedAt)
                .Take(5)
                .ToListAsync() ?? new List<Question>();

            ViewBag.UserReports = await _context.Reports
                .Where(r => r.TargetUserId == id)
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync() ?? new List<Report>();

            // 🔥 DÒNG QUAN TRỌNG NHẤT: Trả về đối tượng user
            // Tuyệt đối KHÔNG trả về danh sách câu hỏi ở đây
            return View(user); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBan(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsBanned = !user.IsBanned;
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Details), new { id = user.Id });
        }
public async Task<IActionResult> ReportedUsers()
{
    var reportedList = await _context.Reports
        .GroupBy(r => r.TargetUserId)
        .Select(g => new ReportedUserViewModel
        {
            UserId = g.Key,
            FullName = _context.Users.Where(u => u.Id == g.Key).Select(u => u.FullName).FirstOrDefault(),
            Email = _context.Users.Where(u => u.Id == g.Key).Select(u => u.Email).FirstOrDefault(),
            // Số người báo cáo khác nhau
            UniqueReporters = g.Select(r => r.ReporterUserId).Distinct().Count(),
            // 🔥 Tổng số lần bị báo cáo
            TotalReports = g.Count(), 
            IsBanned = _context.Users.Where(u => u.Id == g.Key).Select(u => u.IsBanned).FirstOrDefault()
        })
        .OrderByDescending(x => x.TotalReports) // Ưu tiên hiện người bị báo cáo nhiều nhất
        .ToListAsync();

    return View(reportedList);
}
public async Task<IActionResult> ViewReports(string id)
{
    if (string.IsNullOrEmpty(id)) return NotFound();

    var targetUser = await _userManager.FindByIdAsync(id);
    if (targetUser == null) return NotFound();

    // Lấy chi tiết tất cả các báo cáo nhắm vào User này
    var reports = await _context.Reports
        .Where(r => r.TargetUserId == id)
        .Include(r => r.Reporter)   // Người báo cáo
        .Include(r => r.Question)   // Nếu báo cáo câu hỏi
        .Include(r => r.Answer)     // Nếu báo cáo câu trả lời
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync();

    ViewBag.TargetUser = targetUser.FullName;
    return View(reports);
}
    }
}