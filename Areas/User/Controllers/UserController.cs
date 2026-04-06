using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using System.Security.Claims; 
namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; 
        private readonly UserManager<AppUser> _userManager;

        public UserController(AppDbContext context, IWebHostEnvironment env, UserManager<AppUser> userManager)
    {
        _context = context;
        _env = env;
        _userManager = userManager;
    }

    // Trang tổng quan của User
    public IActionResult Index()
    {
        var userId = _userManager.GetUserId(User);
        return RedirectToAction("Profile", new { id = userId });
    }

    // 👤 Hiển thị Profile kèm Tài liệu, Câu hỏi, Câu trả lời
 public async Task<IActionResult> Profile()
{
    var userId = _userManager.GetUserId(User);
    var user = await _context.Users
        .Include(u => u.Documents)
        .Include(u => u.Questions)
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null) return NotFound();

    // Thống kê nhanh cho Dashboard
    ViewBag.TotalDocs = user.Documents.Count;
    ViewBag.TotalQuestions = user.Questions.Count;
    ViewBag.TotalSaved = await _context.SavedDocuments.CountAsync(s => s.UserId == userId);

    return View(user);
}
        [Authorize]
        public IActionResult Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = _context.Users.Find(userId);

            return View(user);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(AppUser model, IFormFile avatarFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = _context.Users.Find(userId);

            if (user == null) return NotFound();

            // update info
            user.FullName = model.FullName;
            user.Email = model.Email;

            // 🔥 upload avatar
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var ext = Path.GetExtension(avatarFile.FileName);
                var fileName = Guid.NewGuid() + ext;

                var path = Path.Combine(_env.WebRootPath, "images", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await avatarFile.CopyToAsync(stream);

                user.Avatar = "/images/" + fileName;
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile", new { id = userId });
        }
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.OldPassword,
                model.NewPassword
            );

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            return RedirectToAction("Profile", new { id = user.Id });
        }
        // Thêm vào UserController.cs

// --- QUẢN LÝ TÀI LIỆU CỦA TÔI ---
        public async Task<IActionResult> MyDocuments()
        {
            var userId = _userManager.GetUserId(User);
            var docs = await _context.Documents
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
            return View(docs);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var userId = _userManager.GetUserId(User);
            var doc = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
            
            if (doc != null)
            {
                // Xóa file vật lý trong thư mục wwwroot
                var filePath = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                _context.Documents.Remove(doc);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(MyDocuments));
        }

// --- QUẢN LÝ CÂU HỎI CỦA TÔI ---
        public async Task<IActionResult> MyQuestions()
        {
            var userId = _userManager.GetUserId(User);
            var questions = await _context.Questions
                .Where(q => q.UserId == userId)
                .Include(q => q.Answers)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
            return View(questions);
        }
        [HttpPost]
public async Task<IActionResult> SaveDocument(int docId)
{
    var userId = _userManager.GetUserId(User);
    
    // Kiểm tra xem đã lưu chưa
    var existing = await _context.SavedDocuments
        .AnyAsync(s => s.UserId == userId && s.DocumentId == docId);

    if (!existing)
    {
        _context.SavedDocuments.Add(new SavedDocument { UserId = userId, DocumentId = docId });
        await _context.SaveChangesAsync();
    }
    return RedirectToAction("Details", "Document", new { id = docId });
}
public async Task<IActionResult> SavedDocuments()
{
    var userId = _userManager.GetUserId(User);
    var savedDocs = await _context.SavedDocuments
        .Where(s => s.UserId == userId)
        .Include(s => s.Document)
        .ThenInclude(d => d.User) // Để hiện tên người đăng gốc
        .OrderByDescending(s => s.SavedDate)
        .ToListAsync();
    return View(savedDocs);
}
    }
}