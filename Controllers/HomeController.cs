using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;

namespace StudyShare.Controllers
{
    // KHÔNG CÓ [Authorize] ở đây để khách có thể vào xem
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // Trang chủ cộng đồng: Ai cũng xem được tài liệu đã phê duyệt
        public async Task<IActionResult> Index(string searchTerm, int? categoryId)
        {
            // Chỉ lấy tài liệu đã được Admin duyệt (IsApproved == true)
            var query = _context.Documents
                .Include(d => d.Category)
                .Include(d => d.User)
                .Where(d => d.IsApproved == true); 

            // Tìm kiếm theo tên hoặc mô tả
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.Title.Contains(searchTerm) || d.Description.Contains(searchTerm));
            }

            // Lọc theo chuyên mục
            if (categoryId.HasValue)
            {
                query = query.Where(d => d.CategoryId == categoryId);
            }

            var docs = await query.OrderByDescending(d => d.UploadDate).ToListAsync();
            
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(docs);
        }

        // Xem chi tiết tài liệu (Công khai)
        public async Task<IActionResult> ViewDocument(int id)
        {
            var doc = await _context.Documents
                .Include(d => d.Category)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsApproved == true);

            if (doc == null) return NotFound();

            // Tăng lượt xem
            doc.Views++;
            _context.Update(doc);
            await _context.SaveChangesAsync();

            return View(doc);
        }
    }
}