using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using System.Security.Claims;
using ai.Services;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class QuestionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AIService _aiService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public QuestionController(AppDbContext context, AIService aiService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _aiService = aiService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var questions = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
            return View(questions);
        }

        public IActionResult Create() => View();

        // ================= CREATE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Answers");

            if (!ModelState.IsValid) return View(question);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // 🔥 AI CHECK
            var aiResult = await _aiService.CheckContentAsync(question.Content);

            if (aiResult.isFlagged)
            {
                user.Points -= 10;
                user.WarningCount += 1;

                if (user.WarningCount > 3)
                {
                    user.IsBanned = true;
                    await _context.SaveChangesAsync();
                    
                    // 🔥 Đăng xuất ngay lập tức và chuyển hướng
                    await _signInManager.SignOutAsync();
                    TempData["Error"] = "Tài khoản của bạn đã bị chặn do vi phạm quá 3 lần!";
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                await _context.SaveChangesAsync();
                ViewBag.Error = $"Bị AI chặn: {aiResult.reason} (-10 điểm, +1 cảnh cáo)";
                return View(question);
            }

            // ✅ OK thì lưu
            question.UserId = userId;
            question.CreatedAt = DateTime.Now;

            user.Points += 5;

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT =================
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

            if (question == null) return NotFound();

            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Question updatedQuestion)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingQuestion = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

            if (existingQuestion == null) return NotFound();

            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Answers");

            if (!ModelState.IsValid) return View(updatedQuestion);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(); // Cẩn thận kiểm tra user null

            // 🔥 AI CHECK
            var aiResult = await _aiService.CheckContentAsync(updatedQuestion.Content);

            if (aiResult.isFlagged)
            {
                user.Points -= 10;
                user.WarningCount += 1;

                if (user.WarningCount > 3)
                {
                    user.IsBanned = true;
                    await _context.SaveChangesAsync();
                    
                    // 🔥 Đăng xuất ngay lập tức và chuyển hướng
                    await _signInManager.SignOutAsync();
                    TempData["Error"] = "Tài khoản của bạn đã bị chặn do vi phạm quá 3 lần!";
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                await _context.SaveChangesAsync();
                ViewBag.Error = $"Bị AI chặn: {aiResult.reason} (-10 điểm, +1 cảnh cáo)";
                return View(updatedQuestion);
            }

            existingQuestion.Content = updatedQuestion.Content;

            _context.Update(existingQuestion);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= ANSWER =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostAnswer(int questionId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", new { id = questionId });

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // 🔥 AI CHECK
            var aiResult = await _aiService.CheckContentAsync(content);

            if (aiResult.isFlagged)
            {
                user.Points -= 10;
                user.WarningCount += 1; // 🔥 Đồng bộ: Tăng 1 lần vi phạm

                // 🔥 Đồng bộ: Tự động khóa tài khoản nếu vi phạm trên 3 lần
                if (user.WarningCount > 3)
                {
                    user.IsBanned = true;
                    await _context.SaveChangesAsync();

                    // 🔥 Đăng xuất ngay lập tức và chuyển hướng
                    await _signInManager.SignOutAsync();
                    TempData["Error"] = "Tài khoản của bạn đã bị chặn do vi phạm quá 3 lần!";
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                await _context.SaveChangesAsync();

                TempData["Error"] = $"Câu trả lời bị chặn: {aiResult.reason} (-10 điểm, +1 cảnh cáo)";
                return RedirectToAction("Details", new { id = questionId });
            }

            var answer = new Answer
            {
                Content = content,
                QuestionId = questionId,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            user.Points += 3;

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = questionId });
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int id)
        {
            var question = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers).ThenInclude(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (question == null) return NotFound();

            return View(question);
        }

        // ================= REPORT =================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(int? questionId, int? answerId, string reason)
        {
            var reporterId = _userManager.GetUserId(User);
            string targetUserId = "";

            if (answerId.HasValue)
            {
                var answer = await _context.Answers.FindAsync(answerId);
                if (answer != null) targetUserId = answer.UserId;
            }
            else if (questionId.HasValue)
            {
                var question = await _context.Questions.FindAsync(questionId);
                if (question != null) targetUserId = question.UserId;
            }

            if (string.IsNullOrEmpty(targetUserId) || reporterId == targetUserId)
            {
                TempData["Error"] = "Thao tác không hợp lệ.";
                return RedirectToAction("Index");
            }

            var report = new Report
            {
                ReporterUserId = reporterId!,
                TargetUserId = targetUserId,
                QuestionId = questionId,
                AnswerId = answerId,
                Reason = reason
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đã gửi báo cáo!";

            int redirectId = questionId ?? (await _context.Answers.FindAsync(answerId))?.QuestionId ?? 0;

            return RedirectToAction("Details", new { id = redirectId });
        }
    }
}