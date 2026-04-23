using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using System.Security.Claims; 
using StudyShare.ViewModels;

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
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            
            var user = await _context.Users
                .Include(u => u.Documents)
                .Include(u => u.Questions)
                .Include(u => u.SavedDocuments)
                .AsNoTracking() // Đảm bảo lấy dữ liệu trực tiếp từ DB
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            ViewBag.TotalDocs = user.Documents?.Count ?? 0;
            ViewBag.TotalQuestions = user.Questions?.Count ?? 0;
            ViewBag.TotalSaved = user.SavedDocuments?.Count ?? 0; 

            return View(user);
        }

        [Authorize]
        public IActionResult Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Find(userId);
            return View(user);
        }

        // 🔥 CẬP NHẬT: Thêm tham số selectedAvatar để nhận tên ảnh được chọn từ giao diện
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(AppUser model, IFormFile avatarFile, string selectedAvatar)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Find(userId);

            if (user == null) return NotFound();

            // update info
            user.FullName = model.FullName;
            user.Email = model.Email;

            // XỬ LÝ AVATAR (Ưu tiên File Upload trước, nếu không có thì lấy Avatar có sẵn)
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var ext = Path.GetExtension(avatarFile.FileName);
                var fileName = Guid.NewGuid() + ext;

                // Thay đổi thư mục lưu nếu cần. Code ban đầu của bạn lưu vào "images", 
                // nhưng avatar hệ thống lại nằm trong "avatar". Cần lưu ý sự đồng bộ này ở giao diện.
                var path = Path.Combine(_env.WebRootPath, "images", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await avatarFile.CopyToAsync(stream);

                user.Avatar = "/images/" + fileName;
            }
            // 🔥 NẾU NGƯỜI DÙNG CHỌN AVATAR CÓ SẴN (Nhấn vào các hình tròn trong thư viện)
            else if (!string.IsNullOrEmpty(selectedAvatar))
            {
                // Lưu thẳng tên file mà người dùng đã chọn (VD: "2.png")
                user.Avatar = selectedAvatar;
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật hồ sơ thành công!";
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

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile", new { id = user.Id });
        }

        // --- QUẢN LÝ CÂU HỎI CỦA TÔI ---
        public async Task<IActionResult> MyQuestions()
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.CurrentUser = await _userManager.FindByIdAsync(userId);

            var questions = await _context.Questions
                .Where(q => q.UserId == userId)
                .Include(q => q.Answers)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
            return View(questions);
        }

        // --- XOÁ TÀI LIỆU CỦA TÔI ---
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var userId = _userManager.GetUserId(User);
            var doc = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
            
            if (doc != null)
            {
                // 1. Xóa các báo cáo liên quan đến tài liệu này
                var relatedReports = _context.Reports.Where(r => r.DocumentId == id);
                _context.Reports.RemoveRange(relatedReports);

                // 2. Xóa các lượt lưu (SavedDocument) của người dùng khác
                var savedEntries = _context.SavedDocuments.Where(s => s.DocumentId == id);
                _context.SavedDocuments.RemoveRange(savedEntries);

                // 3. Xóa file vật lý trên server
                var filePath = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                // 4. Xóa bản ghi tài liệu
                _context.Documents.Remove(doc);
                
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tài liệu của bạn đã được xóa vĩnh viễn.";
            }
            return RedirectToAction(nameof(MyDocuments));
        }

        // --- XOÁ CÂU HỎI CỦA TÔI ---
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var userId = _userManager.GetUserId(User);
            
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

            if (question != null)
            {
                var relatedReports = _context.Reports.Where(r => r.QuestionId == id);
                _context.Reports.RemoveRange(relatedReports);

                var answerIds = _context.Answers.Where(a => a.QuestionId == id).Select(a => a.Id);
                var relatedAnswerReports = _context.Reports.Where(r => r.AnswerId != null && answerIds.Contains(r.AnswerId.Value));
                _context.Reports.RemoveRange(relatedAnswerReports);

                _context.Questions.Remove(question);
                
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa thảo luận và các dữ liệu liên quan thành công.";
            }
            return RedirectToAction(nameof(MyQuestions));
        }

        // --- QUẢN LÝ TÀI LIỆU CỦA TÔI ---
        public async Task<IActionResult> MyDocuments()
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.CurrentUser = await _userManager.FindByIdAsync(userId);

            var docs = await _context.Documents
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
            return View(docs);
        }
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocument(int docId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var existing = await _context.SavedDocuments
                .AnyAsync(s => s.UserId == userId && s.DocumentId == docId);

            if (!existing)
            {
                _context.SavedDocuments.Add(new SavedDocument { 
                    UserId = userId, 
                    DocumentId = docId,
                    SavedDate = DateTime.Now
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã lưu tài liệu vào danh sách của bạn!";
            }
            
            return RedirectToAction("ViewDocument", "Home", new { area = "", id = docId });
        }

        // ✅ BỎ LƯU TÀI LIỆU
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnsaveDocument(int docId)
        {
            var userId = _userManager.GetUserId(User);
            var savedDoc = await _context.SavedDocuments
                .FirstOrDefaultAsync(s => s.UserId == userId && s.DocumentId == docId);

            if (savedDoc != null)
            {
                _context.SavedDocuments.Remove(savedDoc);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã bỏ lưu tài liệu.";
            }

            string returnUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("SavedDocuments"))
            {
                return RedirectToAction(nameof(SavedDocuments));
            }
            return RedirectToAction("ViewDocument", "Home", new { area = "", id = docId });
        }

        // ✅ TRANG HIỂN THỊ DANH SÁCH ĐÃ LƯU
        public async Task<IActionResult> SavedDocuments()
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.CurrentUser = await _userManager.FindByIdAsync(userId);

            var savedDocs = await _context.SavedDocuments
                .Where(s => s.UserId == userId)
                .Include(s => s.Document)
                    .ThenInclude(d => d.User) 
                .OrderByDescending(s => s.SavedDate)
                .ToListAsync();
            return View(savedDocs);
        }

        // --- XOÁ CÂU TRẢ LỜI CỦA TÔI ---
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            var userId = _userManager.GetUserId(User);

            var answer = await _context.Answers
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (answer == null)
            {
                TempData["Error"] = "Bạn không có quyền xoá câu trả lời này hoặc nội dung không tồn tại.";
            }
            else 
            {
                var relatedReports = _context.Reports.Where(r => r.AnswerId == id);
                _context.Reports.RemoveRange(relatedReports);

                _context.Answers.Remove(answer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xoá câu trả lời.";
            }

            string returnUrl = Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index", "Home"); 
            }
            return Redirect(returnUrl);
        }
    }
}