using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using Microsoft.AspNetCore.Authorization;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Xem danh sách danh mục
        // GET: Admin/Category
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        // 2. Tạo mới danh mục (Giao diện)
        // GET: Admin/Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // 3. Lưu danh mục mới vào DB
        // POST: Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // 4. Chỉnh sửa danh mục (Giao diện)
        // GET: Admin/Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            
            return View(category);
        }

        // 5. Cập nhật thay đổi vào DB
        // POST: Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // 6. Xác nhận xóa (Giao diện - Cần cho file Delete.cshtml của bạn)
        // GET: Admin/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (category == null) return NotFound();

            return View(category);
        }

        // 7. Thực hiện xóa khỏi DB
        // POST: Admin/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                // Lưu ý: Nếu có ràng buộc khóa ngoại với bảng Document, 
                // bạn cần xử lý các tài liệu liên quan trước khi xóa ở đây.
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}