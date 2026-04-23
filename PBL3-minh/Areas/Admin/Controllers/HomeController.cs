using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using Microsoft.AspNetCore.Authorization;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy các con số thống kê cho ViewBag
            ViewBag.UserCount = await _context.Users.CountAsync();
            ViewBag.DocCount = await _context.Documents.CountAsync();
            ViewBag.QuestionCount = await _context.Questions.CountAsync();
            
            // 2. Lấy danh sách tài liệu chờ duyệt (Pending)
            var pendingDocs = await _context.Documents
                .Include(d => d.User)
                .Where(d => d.IsApproved == false)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();

            ViewBag.PendingDocsCount = pendingDocs.Count;

            // 🔥 QUAN TRỌNG NHẤT: Phải truyền 'pendingDocs' vào đây để Model không bị NULL
            return View(pendingDocs); 
        }
    }
}