using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
namespace StudyShare.Areas.Admin.Controllers
{
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    private readonly AppDbContext _context;
    public HomeController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        // 🔥 QUAN TRỌNG: Tính toán các con số thống kê
        ViewBag.UserCount = await _context.Users.CountAsync();
        ViewBag.DocCount = await _context.Documents.CountAsync();
        ViewBag.PendingDocs = await _context.Documents.CountAsync(d => !d.IsApproved);
        ViewBag.QuestionCount = await _context.Questions.CountAsync();

        // Nếu View của bạn dùng @Model.Count(), bạn phải truyền một danh sách vào đây
        // Ví dụ: var docs = await _context.Documents.ToListAsync();
        // return View(docs); 

        return View(); // Nếu dùng ViewBag thì trả về View không kèm Model
    }
}
}