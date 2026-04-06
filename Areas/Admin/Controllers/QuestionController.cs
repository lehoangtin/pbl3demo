using Microsoft.AspNetCore.Authorization;
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

        public QuestionController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách tất cả câu hỏi
       public async Task<IActionResult> Index()
{
    var questions = await _context.Questions
        .Include(q => q.User) // Phải có dòng này để load thông tin người đăng câu hỏi
        .ToListAsync();
    return View(questions);
}

        // 2. Xem chi tiết câu hỏi và các câu trả lời đi kèm
        public async Task<IActionResult> Details(int id)
        {
            var question = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers).ThenInclude(a => a.User) // Lấy câu trả lời và người trả lời
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null) return NotFound();

            return View(question);
        }

        // 3. Xóa câu hỏi (Xóa luôn các câu trả lời liên quan do Cascade Delete)
        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 4. Xóa một câu trả lời cụ thể
        [HttpPost]
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