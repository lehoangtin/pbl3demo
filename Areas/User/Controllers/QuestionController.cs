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

        // 1. Trang danh sách câu hỏi (Ai cũng xem được)
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

        // 2. Giao diện đặt câu hỏi
        public IActionResult Create()
        {
            return View();
        }

        // 3. Xử lý đặt câu hỏi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question)
        {
            // 🔥 Loại bỏ kiểm tra UserId/User để tránh lỗi ModelState.IsValid = false
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Challenge();

                question.UserId = userId; 
                question.CreatedAt = DateTime.Now;

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            return View(question);
        }

        // 4. Xem chi tiết câu hỏi và trả lời
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

        // 5. Gửi câu trả lời mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostAnswer(int questionId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) 
                return RedirectToAction("Details", new { id = questionId });

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var answer = new Answer
            {
                Content = content,
                QuestionId = questionId,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = questionId });
        }
    }
}