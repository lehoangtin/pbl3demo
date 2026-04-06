using Microsoft.AspNetCore.Mvc;
using StudyShare.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize] // Bảo vệ toàn bộ Controller, yêu cầu đăng nhập
    public class DocumentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        private readonly string[] allowedExtensions = { ".pdf", ".docx", ".pptx", ".xlsx" };
        private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB

        public DocumentController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. Chỉ xem tài liệu của chính mình
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var docs = await _context.Documents
                .Where(d => d.UserId == userId)
                .Include(d => d.Category)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
            return View(docs);
        }

        // 2. Giao diện tải lên
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // 3. Xử lý tải lên (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Document doc, IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            // Kiểm tra file thủ công vì bạn đã khai báo ràng buộc
            if (file == null || file.Length == 0)
                ModelState.AddModelError("file", "Vui lòng chọn một tệp tin.");
            else
            {
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                    ModelState.AddModelError("file", "Chỉ chấp nhận file PDF, Word, Excel, PowerPoint.");
                
                if (file.Length > MAX_FILE_SIZE)
                    ModelState.AddModelError("file", "Dung lượng file không được vượt quá 10MB.");
            }

            if (ModelState.IsValid)
            {
                // Xử lý lưu file
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Gán dữ liệu cho Model
                doc.UserId = userId;
                doc.UploadDate = DateTime.Now;
                doc.IsApproved = false; // Chờ Admin duyệt
                doc.FilePath = "/uploads/" + uniqueFileName;
                doc.FileName = file.FileName;
                doc.FileType = file.ContentType;
                doc.Views = 0; // Đảm bảo không bị null

                _context.Add(doc);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", doc.CategoryId);
            return View(doc);
        }

        // 4. Xóa tài liệu (Có kiểm tra quyền sở hữu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doc = await _context.Documents.FindAsync(id);

            if (doc == null) return NotFound();
            
            // Bảo mật: Chỉ cho phép xóa nếu là chủ sở hữu
            if (doc.UserId != userId) return Forbid();

            // Xóa file vật lý
            var path = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 5. Xem chi tiết
        public async Task<IActionResult> Details(int id)
        {
            var doc = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (doc == null) return NotFound();
            return View(doc);
        }

        // 6. Download
        public async Task<IActionResult> Download(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();

            doc.DownloadCount++;
            _context.Update(doc);
            await _context.SaveChangesAsync();

            var filePath = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));
            return PhysicalFile(filePath, "application/octet-stream", doc.FileName);
        }
    }
}