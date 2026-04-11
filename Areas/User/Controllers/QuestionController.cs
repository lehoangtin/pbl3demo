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

        // Cập nhật trong hàm Create (Post) của Question
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

        question.UserId = userId; 
        question.CreatedAt = DateTime.Now;

        // Cộng 5 điểm cho người đặt câu hỏi
        var user = await _context.Users.FindAsync(userId);
        if (user != null) user.Points += 5;

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    return View(question);
}
// --- THÊM VÀO QuestionController.cs ---

// 1. Giao diện chỉnh sửa (GET)
public async Task<IActionResult> Edit(int id)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

    if (question == null) return NotFound();
    return View(question);
}

// 2. Xử lý cập nhật (POST)
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Question updatedQuestion)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var existingQuestion = await _context.Questions.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

    if (existingQuestion == null) return NotFound();

    ModelState.Remove("UserId");
    ModelState.Remove("User");
    ModelState.Remove("Answers");

    if (ModelState.IsValid)
    {
        existingQuestion.Content = updatedQuestion.Content;
        // Có thể giữ nguyên ngày tạo cũ hoặc cập nhật ngày sửa nếu muốn
        
        _context.Update(existingQuestion);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    return View(updatedQuestion);
}
// Cập nhật trong hàm PostAnswer
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

    // Cộng 3 điểm cho người trả lời
    var user = await _context.Users.FindAsync(userId);
    if (user != null) user.Points += 3;

    _context.Answers.Add(answer);
    await _context.SaveChangesAsync();
    return RedirectToAction("Details", new { id = questionId });
}
        public async Task<IActionResult> Details(int id)
        {
            var question = await _context.Questions
                .Include(q => q.User) 
                .Include(q => q.Answers).ThenInclude(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id); // Đã sửa: dùng .Id

            if (question == null) return NotFound();
            return View(question);
        }

[HttpPost]
[Authorize]
[ValidateAntiForgeryToken]
[HttpPost]
[Authorize]
public async Task<IActionResult> Report(int? questionId, int? answerId, string reason)
{
    var reporterId = _userManager.GetUserId(User);
    string targetUserId = "";

    if (answerId.HasValue) // Ưu tiên kiểm tra báo cáo câu trả lời trước
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
        ReporterUserId = reporterId,
        TargetUserId = targetUserId,
        QuestionId = questionId,
        AnswerId = answerId,
        Reason = reason
    };

    _context.Reports.Add(report);
    await _context.SaveChangesAsync();

    TempData["Message"] = "Cảm ơn bạn! Báo cáo đã được gửi tới Quản trị viên.";
    
    // Quay lại trang chi tiết câu hỏi
    int redirectId = questionId ?? (await _context.Answers.FindAsync(answerId)).QuestionId;
    return RedirectToAction("Details", new { id = redirectId });
}
    }
}