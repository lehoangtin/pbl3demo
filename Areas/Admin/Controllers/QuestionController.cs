using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class QuestionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager; // Thêm khai báo này

        public QuestionController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager; // 🔥 Tiêm vào constructor
        }

        // 1. Danh sách tất cả câu hỏi
        public async Task<IActionResult> Index()
        {
            var questions = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
            return View(questions);
        }

        // 2. Xem chi tiết câu hỏi và các báo cáo liên quan
        public async Task<IActionResult> Details(int id)
        {
            var question = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers).ThenInclude(a => a.User)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null) return NotFound();

            // 🔥 Lấy danh sách báo cáo của câu hỏi này để Admin xem xét vi phạm
            ViewBag.Reports = await _context.Reports
                .Where(r => r.QuestionId == id)
                .Include(r => r.Reporter)
                .ToListAsync();

            return View(question);
        }

        // 3. Xóa câu hỏi (Xóa luôn các câu trả lời liên quan)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                // Xóa cả các báo cáo liên quan đến câu hỏi này để tránh lỗi FK
                var reports = _context.Reports.Where(r => r.QuestionId == id);
                _context.Reports.RemoveRange(reports);

                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 4. Xóa câu trả lời vi phạm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnswer(int id, int questionId)
        {
            var answer = await _context.Answers.FindAsync(id);
            if (answer != null)
            {
                _context.Answers.Remove(answer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = questionId });
        }
    }
}