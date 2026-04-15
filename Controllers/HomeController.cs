using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using Microsoft.AspNetCore.Authorization; 
using System.Security.Claims; 
using Microsoft.AspNetCore.Identity; 

namespace StudyShare.Controllers
{
    // KHÔNG CÓ [Authorize] ở mức Class để khách có thể vào xem hàm Index
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SignInManager<AppUser> _signInManager; // 🔥 Sửa lại viết hoa chữ S
        private readonly UserManager<AppUser> _userManager;     // 🔥 KHAI BÁO THÊM UserManager

        // 🔥 TIÊM THÊM UserManager VÀO CONSTRUCTOR
        public HomeController(AppDbContext context, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ==========================================
        // TRANG CHỦ & TÌM KIẾM (AI CŨNG XEM ĐƯỢC)
        // ==========================================
        public async Task<IActionResult> Index(string searchTerm, int? categoryId)
        {
            // 🔥 KIỂM TRA BANNED KHI ĐÃ ĐĂNG NHẬP
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.IsBanned)
            {
                await _signInManager.SignOutAsync();
                TempData["Error"] = "Tài khoản của bạn đã bị khóa!";
                return RedirectToAction("Login", "Account", new { area = "" }); 
            }

            // 1. Khởi tạo query lấy tài liệu đã duyệt
            var query = _context.Documents
                .Include(d => d.Category)
                .Include(d => d.User)
                .Where(d => d.IsApproved == true); 

            // 2. Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.Title.Contains(searchTerm) || d.Description.Contains(searchTerm));
                ViewBag.SearchTerm = searchTerm; 
            }

            // 3. Lọc theo chuyên mục
            if (categoryId.HasValue)
            {
                query = query.Where(d => d.CategoryId == categoryId);
                ViewBag.CurrentCategory = categoryId; 
            }

            var docs = await query.OrderByDescending(d => d.UploadDate).ToListAsync();
            
            // Lấy danh sách category để hiển thị menu lọc
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(docs);
        }

        // ==========================================
        // XEM CHI TIẾT (BẮT BUỘC PHẢI ĐĂNG NHẬP)
        // ==========================================
        [Authorize] 
        public async Task<IActionResult> ViewDocument(int id)
        {
            var doc = await _context.Documents
                .Include(d => d.Category)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsApproved == true);

            if (doc == null) return NotFound();
            
            // 1. Lấy ID của User đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Tạo một tên Cookie duy nhất
            string viewCookieName = $"Viewed_Doc_{id}_User_{userId}";

            // 3. Kiểm tra xem trình duyệt của người này đã có Cookie này chưa
            if (!Request.Cookies.ContainsKey(viewCookieName))
            {
                // Nếu CHƯA CÓ -> Tăng lượt xem
                doc.Views++;
                _context.Update(doc);
                await _context.SaveChangesAsync();

                // Tạo Cookie lưu vào trình duyệt của người dùng
                CookieOptions options = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true
                };
                Response.Cookies.Append(viewCookieName, "seen", options);
            }
            
            return View(doc);
        }
    }
}