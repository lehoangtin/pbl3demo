using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using Microsoft.AspNetCore.Authorization;

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

        // 📋 Danh sách toàn bộ tài liệu (có tìm kiếm)
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Documents.Include(d => d.User).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.Title.Contains(search));
            }

            return View(await query.OrderByDescending(d => d.UploadDate).ToListAsync());
        }

        // ✅ Phê duyệt tài liệu và cộng 10 điểm cho người đăng
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Approve(int id)
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
        TempData["Success"] = "Tài liệu đã được phê duyệt và người dùng được cộng 10đ!";
    }

    return RedirectToAction(nameof(Index));
}
        // 🗑️ Xóa tài liệu (Xóa cả file vật lý)
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

            return RedirectToAction(nameof(Index));
        }
    }
}