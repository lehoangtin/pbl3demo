using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            // 1. Lấy thông tin User
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // 2. Lấy danh sách Vai trò
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles.Any() ? string.Join(", ", roles) : "Thành viên";

            // 3. Thống kê số lượng
            ViewBag.DocumentCount = await _context.Documents.CountAsync(d => d.UserId == id);
            ViewBag.QuestionCount = await _context.Questions.CountAsync(q => q.UserId == id);
            ViewBag.AnswerCount = await _context.Answers.CountAsync(a => a.UserId == id);

            // 4. Lấy dữ liệu liên quan và bỏ vào ViewBag
            ViewBag.RecentDocuments = await _context.Documents
                .Where(d => d.UserId == id)
                .OrderByDescending(d => d.UploadDate) 
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
            // 🔥 ĐÃ FIX LỖI Ở ĐÂY: Đổi ReportedUser thành Target
            var reports = await _context.Reports
                .Include(r => r.Target)  
                .Include(r => r.Reporter) 
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Chia làm 2 danh sách truyền ra View bằng ViewBag
            ViewBag.PendingReports = reports.Where(r => !r.IsResolved).ToList();
            ViewBag.HistoryReports = reports.Where(r => r.IsResolved).ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DismissReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                report.IsResolved = true; // Đánh dấu đã giải quyết
                report.ActionTaken = "Bỏ qua (Không có vi phạm)"; // Lưu lịch sử thao tác
                
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã bỏ qua báo cáo và lưu vào lịch sử!";
            }
            return RedirectToAction(nameof(ReportedUsers));
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

                // Tự động ban nếu điểm dưới 0 hoặc vi phạm từ 3 lần trở lên
                if (targetUser.WarningCount >= 3 || targetUser.Points < 0)
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

        // --- ACTION SỬA THÔNG TIN NGƯỜI DÙNG ---
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AppUser model)
        {
            if (id != model.Id) return NotFound();
            
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Cập nhật các thông tin cần thiết
            user.FullName = model.FullName;
            user.Points = model.Points;
            user.WarningCount = model.WarningCount;
            user.IsBanned = model.IsBanned;
            
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BanUser(string userId, int reportId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsBanned = true; // Khóa User
                
                // Cập nhật trạng thái báo cáo
                var report = await _context.Reports.FindAsync(reportId);
                if (report != null)
                {
                    report.IsResolved = true;
                    report.ActionTaken = "Đã khóa tài khoản vi phạm";
                }
                
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã khóa tài khoản của {user.FullName} và lưu lịch sử!";
            }
            return RedirectToAction(nameof(ReportedUsers));
        }
    }
}