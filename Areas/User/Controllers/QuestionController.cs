using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using System.Security.Claims;
using ai.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class QuestionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AIService _aiService;
        private readonly UserManager<AppUser> _userManager;

        public QuestionController(AppDbContext context, AIService aiService, UserManager<AppUser> userManager)
        {
            _context = context;
            _aiService = aiService;
            _userManager = userManager;
        }

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

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Answers");

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Challenge();

                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound();
                var textToCheck = question.Content;
                var aiResult = await _aiService.CheckContentAsync(textToCheck);

                if (aiResult.isFlagged)
                {
                    int penaltyPoints = 10; 
                    user.Points -= penaltyPoints;

                    // Lưu lịch sử phạt điểm 
                    /*
                    _context.PointTransactions.Add(new PointTransaction
                    {
                        UserId = userId,
                        Amount = penaltyPoints,
                        Type = TransactionType.Penalty,
                        Description = $"Vi phạm ngôn từ: {aiResult.reason}"
                    });
                    */

                    await _context.SaveChangesAsync();

                    // Báo lỗi ra màn hình và chặn
                    ViewBag.Error = $"Nội dung của bạn đã bị AI chặn! Lý do: {aiResult.reason}. Bạn bị trừ {penaltyPoints} điểm.";
                    return View(question);
                }

                // Nếu AI cho qua thì lưu bài bình thường
                question.UserId = userId; 
                question.CreatedAt = DateTime.Now;

                // Cộng 5 điểm cho người đặt câu hỏi
                user.Points += 5;

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(question);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

            if (question == null) return NotFound();
            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Question updatedQuestion)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingQuestion = await _context.Questions.FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

            if (existingQuestion == null) return NotFound();

            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Answers");

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(userId);
                var textToCheck = updatedQuestion.Content;
                var aiResult = await _aiService.CheckContentAsync(textToCheck);

                if (aiResult.isFlagged)
                {
                    if (user != null)
                    {
                        int penaltyPoints = 10;
                        user.Points -= penaltyPoints;
                        await _context.SaveChangesAsync();
                    }

                    ViewBag.Error = $"Nội dung chỉnh sửa đã bị AI chặn! Lý do: {aiResult.reason}. Bạn bị trừ 10 điểm.";
                    return View(updatedQuestion);
                }

                // existingQuestion.Title = updatedQuestion.Title; // Mở comment nếu cập nhật cả Title
                existingQuestion.Content = updatedQuestion.Content;
                
                _context.Update(existingQuestion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(updatedQuestion);
        }

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

            // --- 1. GỌI AI KIỂM DUYỆT TRƯỚC KHI LƯU ---
            var aiResult = await _aiService.CheckContentAsync(content);

            if (aiResult.isFlagged)
            {
                int penaltyPoints = 10;
                user.Points -= penaltyPoints;
                await _context.SaveChangesAsync();
                TempData["Error"] = $"Câu trả lời của bạn đã bị AI chặn! Lý do: {aiResult.reason}. Bạn bị trừ {penaltyPoints} điểm.";
                return RedirectToAction("Details", new { id = questionId });
            }
            var answer = new Answer
            {
                Content = content,
                QuestionId = questionId,
                UserId = userId,
                CreatedAt = DateTime.Now
            };
            user.Points += 2;

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = questionId });
        }

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

            if (string.IsNullOrEmpty(targetUserId) || string.IsNullOrEmpty(reporterId) || reporterId == targetUserId)
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

            TempData["Message"] = "Cảm ơn bạn! Báo cáo đã được gửi tới Quản trị viên.";

            int redirectId = questionId ?? (await _context.Answers.FindAsync(answerId))?.QuestionId ?? 0;

            return RedirectToAction("Details", new { id = redirectId });
        }
    }
}