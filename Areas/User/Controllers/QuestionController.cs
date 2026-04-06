using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using System.Security.Claims;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class QuestionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public QuestionController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 📋 Danh sách tất cả câu hỏi thảo luận
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var questions = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
            return View(questions);
        }

        // 🔍 Chi tiết câu hỏi và danh sách câu trả lời
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var question = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers).ThenInclude(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (question == null) return NotFound();
            return View(question);
        }

        // ➕ Đăng câu hỏi mới (View)
        public IActionResult Create() => View();

        // ➕ Đăng câu hỏi mới (Xử lý)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question)
        {
            question.UserId = _userManager.GetUserId(User);
            question.CreatedAt = DateTime.Now;
            
            _context.Add(question);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 💬 Gửi câu trả lời
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostAnswer(int questionId, string content)
        {
            if (string.IsNullOrEmpty(content)) return RedirectToAction("Details", new { id = questionId });

            var answer = new Answer
            {
                Content = content,
                QuestionId = questionId,
                UserId = _userManager.GetUserId(User),
                CreatedAt = DateTime.Now
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = questionId });
        }
    }
}