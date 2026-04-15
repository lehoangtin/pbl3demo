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

// Areas/Admin/Controllers/UserController.cs

public async Task<IActionResult> Index(string searchString)
{
    // Lấy truy vấn cơ sở từ UserManager
    var users = _userManager.Users;

    // Nếu có từ khóa tìm kiếm, thực hiện lọc
    if (!string.IsNullOrEmpty(searchString))
    {
        users = users.Where(u => u.FullName.Contains(searchString) || 
                                 u.Email.Contains(searchString));
    }

    // Lưu lại từ khóa để hiển thị lại trên ô nhập liệu (UI)
    ViewData["CurrentFilter"] = searchString;

    return View(await users.ToListAsync());
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
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(string id)
{
    if (string.IsNullOrEmpty(id)) return NotFound();

    var user = await _userManager.FindByIdAsync(id);
    if (user == null) return NotFound();

    // 1. Xoá tất cả báo cáo liên quan (cả người báo cáo và người bị báo cáo)
    var relatedReports = _context.Reports.Where(r => r.ReporterUserId == id || r.TargetUserId == id);
    _context.Reports.RemoveRange(relatedReports);

    // 2. Xoá các tài liệu đã lưu
    var savedDocs = _context.SavedDocuments.Where(sd => sd.UserId == id);
    _context.SavedDocuments.RemoveRange(savedDocs);

    // 3. Xoá các câu trả lời
    var userAnswers = _context.Answers.Where(a => a.UserId == id);
    _context.Answers.RemoveRange(userAnswers);

    // 4. Xoá các câu hỏi
    var userQuestions = _context.Questions.Where(q => q.UserId == id);
    _context.Questions.RemoveRange(userQuestions);

    // 5. Xoá các tài liệu đã tải lên
    var userDocs = _context.Documents.Where(d => d.UserId == id);
    _context.Documents.RemoveRange(userDocs);

    // Lưu các thay đổi ở bảng phụ trước
    await _context.SaveChangesAsync();

    // 6. Cuối cùng mới xoá User
    var result = await _userManager.DeleteAsync(user);

    if (result.Succeeded)
    {
        return RedirectToAction(nameof(Index));
    }

    foreach (var error in result.Errors)
    {
        ModelState.AddModelError("", error.Description);
    }
    return View("Details", user);
}
// Xử lý báo cáo: Trừ 5đ và tăng 1 lần cảnh cáo
// Xử lý báo cáo: Trừ 5đ và tăng 1 lần cảnh cáo
[HttpPost]
[Authorize(Roles = "Admin")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ConfirmReport(int reportId)
{
    var report = await _context.Reports
        .Include(r => r.Target)
        .FirstOrDefaultAsync(r => r.Id == reportId);

    if (report == null) return NotFound();

    var targetUser = report.Target;
    if (targetUser != null)
    {
        targetUser.Points -= 5; // Trừ 5 điểm
        targetUser.WarningCount += 1; // Tăng số lần cảnh cáo

        // 🔥 SỬA TẠI ĐÂY: Trên 3 lần vi phạm (> 3) thì ban tài khoản
        // (Nếu ý bạn là 3 lần vi phạm là ban luôn thì giữ nguyên >= 3 nhé)
        if (targetUser.WarningCount > 3)
        {
            targetUser.IsBanned = true;
        }

        // Xóa báo cáo sau khi xử lý hoặc đánh dấu đã xử lý
        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Đã xử lý báo cáo. {targetUser.FullName} bị trừ 5đ và nhận 1 cảnh cáo.";
    }

    return RedirectToAction("ViewReports", new { id = report.TargetUserId });
}
    }
}