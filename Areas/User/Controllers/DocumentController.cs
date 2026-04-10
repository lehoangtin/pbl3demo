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
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        // Cấu hình các định dạng file cho phép
        private readonly string[] allowedExtensions = { ".pdf", ".docx", ".pptx", ".xlsx", ".txt", ".png", ".jpg" };
        private const long MAX_FILE_SIZE = 20 * 1024 * 1024; // Nâng lên 20MB cho thoải mái

        public DocumentController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. Danh sách tài liệu cá nhân
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
            // Lấy ID người dùng hiện tại
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            // QUAN TRỌNG: Gán ID ngay lập tức để sử dụng
            doc.UserId = userId;

            // Kiểm tra file thủ công
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Vui lòng chọn một tệp tin để tải lên.");
            }
            else
            {
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("file", "Định dạng file không hỗ trợ (Chỉ nhận PDF, Word, Excel, PowerPoint, Ảnh).");
                }
                
                if (file.Length > MAX_FILE_SIZE)
                {
                    ModelState.AddModelError("file", "File quá lớn. Dung lượng tối đa là 20MB.");
                }
            }

            // Xóa validation lỗi cho các trường hệ thống tự sinh
            // Việc này giúp ModelState.IsValid không bị false do thiếu UserId hay FilePath từ Form
            ModelState.Remove("UserId");
            ModelState.Remove("FilePath");
            ModelState.Remove("FileName");
            ModelState.Remove("FileType");
            ModelState.Remove("User"); // Xóa bỏ kiểm tra Navigation Property nếu có
            ModelState.Remove("Category"); // 🔥 CẦN THÊM DÒNG NÀY
            if (ModelState.IsValid)
            {
                try 
                {
                    // Xử lý lưu file vật lý
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    // Hoàn thiện thông tin Model
                    doc.FilePath = "/uploads/" + uniqueFileName;
                    doc.FileName = file.FileName;
                    doc.FileType = file.ContentType;
                    doc.FileSize = file.Length; // 🔥 THÊM DÒNG NÀY để lưu dung lượng file
                    doc.UploadDate = DateTime.Now;
                    doc.IsApproved = false; // Chờ Admin duyệt
                    doc.Views = 0;
                    doc.DownloadCount = 0;
                    

                    _context.Add(doc);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Tải lên thành công! Tài liệu của bạn đang chờ quản trị viên phê duyệt.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình lưu dữ liệu: " + ex.Message);
                }
            }

            // Nếu có lỗi, chuẩn bị lại dữ liệu cho View
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", doc.CategoryId);
            return View(doc);
        }

        // 4. Xóa tài liệu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doc = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (doc == null) return NotFound();

            try 
            {
                // Xóa file vật lý trước
                var path = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

                _context.Documents.Remove(doc);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa tài liệu thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể xóa tài liệu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // 5. Xem chi tiết tài liệu cá nhân
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doc = await _context.Documents
                .Include(d => d.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (doc == null) return NotFound();
            return View(doc);
        }

        // 6. Tải xuống file
        public async Task<IActionResult> Download(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();

            // Tăng lượt tải
            doc.DownloadCount++;
            _context.Update(doc);
            await _context.SaveChangesAsync();

            var filePath = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath)) return NotFound("File không tồn tại trên hệ thống.");

            return PhysicalFile(filePath, "application/octet-stream", doc.FileName);
        }
    }
}