using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyShare.Models;
using Microsoft.EntityFrameworkCore;
namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context; // 🔥 Khai báo thêm database context

        public UserController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // 🔥 DELETE USER
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction("Index");
        }

        // 2. Action Khóa User
        // Tệp: Areas/Admin/Controllers/UserController.cs
        [HttpPost]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Nếu đang bị khóa thì mở, nếu đang mở thì khóa 100 năm
            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
                await _userManager.SetLockoutEndDateAsync(user, null);
            else
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(100));

            return RedirectToAction(nameof(Index));
        }
// Sửa lỗi Details - Không dùng System.Data.Entity
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // Lấy thông tin User
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Lấy thống kê từ Database sử dụng Microsoft.EntityFrameworkCore
            ViewBag.DocumentCount = await _context.Documents.CountAsync(d => d.UserId == id);
            ViewBag.QuestionCount = await _context.Questions.CountAsync(q => q.UserId == id);
            ViewBag.AnswerCount = await _context.Answers.CountAsync(a => a.UserId == id);

            return View(user);
        }
    }
}
