using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using System.Security.Claims;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    public class QuestionController : Controller
    {
        private readonly AppDbContext _context;

        public QuestionController(AppDbContext context)
        {
            _context = context;
        }
        
        // 📄 Danh sách câu hỏi
        public IActionResult Index()
        {
            var questions = _context.Questions
                .Include(q => q.User)        // 🔥 thêm dòng này
                .Include(q => q.Answers)     // 🔥 thêm dòng này
                .ToList();

            return View(questions);
        }

        // ❓ Trang tạo câu hỏi
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Question q)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            q.UserId = userId;

            _context.Questions.Add(q);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [HttpPost]

        // 📄 Chi tiết câu hỏi
        public IActionResult Details(int id)
        {
            var q = _context.Questions
                .Include(q => q.User)                    // user hỏi
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)           // 🔥 user trả lời
                .FirstOrDefault(q => q.Id == id);

            if (q == null) return NotFound();

            return View(q);
        }

        // 💬 Trả lời
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Answer(int questionId, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ans = new Answer
            {
                QuestionId = questionId,
                Content = content,
                UserId = userId
            };

            _context.Answers.Add(ans);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = questionId });
        }
    }
}