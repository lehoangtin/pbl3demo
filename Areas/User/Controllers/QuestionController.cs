using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<AppUser> _userManager;

    public QuestionController(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }// GET: User/Question/Create
[Authorize]
public IActionResult Create()
{
    return View();
}

// POST: User/Question/Create
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize]
public async Task<IActionResult> Create(Question question)
{
    if (ModelState.IsValid)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        // 🔥 Gán UserId cho câu hỏi
        question.UserId = userId; 
        question.CreatedAt = DateTime.Now;

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }
    return View(question);
}

    // Hiển thị danh sách tất cả câu hỏi
    public async Task<IActionResult> Index()
    {
        var questions = await _context.Questions
            .Include(q => q.User)
            .Include(q => q.Answers)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
        return View(questions);
    }

    // Xem chi tiết câu hỏi và các câu trả lời
    public async Task<IActionResult> Details(int id)
    {
        var question = await _context.Questions
            .Include(q => q.User)
            .Include(q => q.Answers).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (question == null) return NotFound();
        return View(question);
    }

    // Gửi câu trả lời mới
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostAnswer(int questionId, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return RedirectToAction("Details", new { id = questionId });

        var userId = _userManager.GetUserId(User);
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