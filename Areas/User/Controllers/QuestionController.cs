using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using System.Security.Claims;
using ai.Services;
using System;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Create(Question model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // 1. GỌI AI KIỂM TRA NỘI DUNG TRƯỚC KHI LƯU
            var aiResult = await _aiService.CheckContentAsync(model.Content);

            // 2. NẾU AI PHÁT HIỆN VI PHẠM
            if (aiResult.isFlagged)
            {
                user.Points -= 10;
                user.WarningCount += 1;

                // Lưu thẳng báo cáo vào Lịch sử xử lý
                var newReport = new Report
                {
                    TargetUserId = userId,
                    ReporterUserId = null, // Logo AI
                    Reason = $"AI Quét tự động (Tạo câu hỏi): {aiResult.reason}",
                    CreatedAt = DateTime.Now,
                    IsResolved = true, 
                    ActionTaken = "Hệ thống AI chặn bài & Phạt 10đ" 
                };
                _context.Reports.Add(newReport);

                // Khóa tài khoản nếu vi phạm nhiều
                if (user.WarningCount >= 3 || user.Points < 0)
                {
                    user.IsBanned = true;
                    await _context.SaveChangesAsync();
                    
                    await _signInManager.SignOutAsync();
                    TempData["Error"] = "Tài khoản của bạn đã bị khóa do vi phạm quá 3 lần hoặc điểm âm!";
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                await _context.SaveChangesAsync();

                ModelState.AddModelError("", $"Nội dung vi phạm: {aiResult.reason} (-10 điểm, +1 cảnh cáo)");
                return View(model); 
            }

            // 3. NẾU AN TOÀN THÌ LƯU CÂU HỎI
            model.UserId = userId;
            model.CreatedAt = DateTime.Now;
            
            _context.Questions.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng câu hỏi thành công!";
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
            if (user == null) return NotFound();

            // 🔥 AI CHECK
            var aiResult = await _aiService.CheckContentAsync(updatedQuestion.Content);

            if (aiResult.isFlagged)
            {
                user.Points -= 10;
                user.WarningCount += 1;

                // Lưu thẳng báo cáo vào Lịch sử xử lý
                var newReport = new Report
                {
                    TargetUserId = userId,
                    ReporterUserId = null,
                    QuestionId = id,
                    Reason = $"AI Quét tự động (Sửa câu hỏi): {aiResult.reason}",
                    CreatedAt = DateTime.Now,
                    IsResolved = true,
                    ActionTaken = "Hệ thống AI chặn sửa bài & Phạt 10đ"
                };
                _context.Reports.Add(newReport);

                if (user.WarningCount >= 3 || user.Points < 0)
                {
                    user.IsBanned = true;
                    await _context.SaveChangesAsync();
                    
                    await _signInManager.SignOutAsync();
                    TempData["Error"] = "Tài khoản của bạn đã bị khóa do vi phạm quá nhiều!";
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                await _context.SaveChangesAsync();
                ViewBag.Error = $"Bị AI chặn: {aiResult.reason} (-10 điểm, +1 cảnh cáo)";
                return View(updatedQuestion);
            }

            existingQuestion.Content = updatedQuestion.Content;

            _context.Update(existingQuestion);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật câu hỏi thành công!";
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
                user.WarningCount += 1;

                // Lưu thẳng báo cáo vào Lịch sử xử lý
                var newReport = new Report
                {
                    TargetUserId = userId,
                    ReporterUserId = null,
                    QuestionId = questionId,
                    Reason = $"AI Quét tự động (Bình luận): {aiResult.reason}",
                    CreatedAt = DateTime.Now,
                    IsResolved = true,
                    ActionTaken = "Hệ thống AI chặn bình luận & Phạt 10đ"
                };
                _context.Reports.Add(newReport);

                if (user.WarningCount >= 3 || user.Points < 0)
                {
                    user.IsBanned = true;
                    await _context.SaveChangesAsync();

                    await _signInManager.SignOutAsync();
                    TempData["Error"] = "Tài khoản của bạn đã bị khóa do vi phạm quá 3 lần!";
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

            TempData["Success"] = "Đã gửi câu trả lời và được cộng 3 điểm!";
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

        // ================= REPORT (USER BÁO CÁO NHAU) =================
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
                Reason = reason,
                CreatedAt = DateTime.Now,
                // Báo cáo do User gửi thì Admin vẫn phải duyệt (chưa giải quyết)
                IsResolved = false,
                ActionTaken = null 
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đã gửi báo cáo vi phạm thành công! Quản trị viên sẽ xem xét.";

            int redirectId = questionId ?? (await _context.Answers.FindAsync(answerId))?.QuestionId ?? 0;

            return RedirectToAction("Details", new { id = redirectId });
        }
    }
}