using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được vào
    public class DocumentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DocumentController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 📋 1. Danh sách toàn bộ tài liệu (có tìm kiếm)
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Documents.Include(d => d.User).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.Title.Contains(search));
            }

            return View(await query.OrderByDescending(d => d.UploadDate).ToListAsync());
        }

        // 🔍 2. Hiển thị trang Xem & Duyệt dành riêng cho Admin
        public async Task<IActionResult> Review(int id)
        {
            var document = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (document == null) return NotFound();

            return View(document);
        }

        // ✅ 3. Phê duyệt tài liệu và cộng 10 điểm cho người đăng 
        // (Đổi tên thành ApproveDocument để khớp với nút submit bên file Review.cshtml)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDocument(int id)
        {
            var doc = await _context.Documents.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
            if (doc == null) return NotFound();

            if (!doc.IsApproved) // Chỉ cộng điểm nếu tài liệu chưa được duyệt trước đó
            {
                doc.IsApproved = true;
                
                // Cộng 10 điểm cho người đăng
                if (doc.User != null)
                {
                    doc.User.Points += 10;
                }

                _context.Update(doc);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tài liệu đã được phê duyệt thành công và người dùng đã được cộng 10đ!";
            }

            // Sau khi duyệt xong, trả Admin về trang Dashboard
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        // 🗑️ 4. Xóa tài liệu (Xóa cả file vật lý)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();

            // 1. Xóa file trong thư mục wwwroot
            var filePath = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // 2. Xóa trong Database
            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa / từ chối tài liệu thành công!";
            
            // Xoá xong trả về trang Dashboard (hoặc bạn có thể đổi thành trả về Index của Document)
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
    }
}