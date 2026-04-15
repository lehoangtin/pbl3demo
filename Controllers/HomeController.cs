using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using Microsoft.AspNetCore.Authorization; 
using System.Security.Claims; 
using Microsoft.AspNetCore.Identity; 
using System.Security.Claims; // Thêm dòng này ở đầu file nếu chưa có
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
    var document = await _context.Documents.FindAsync(id);
    if (document == null) return NotFound();

    // 1. Kiểm tra xem người dùng đã đăng nhập chưa
    bool isSaved = false;
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
    if (userId != null)
    {
        // 2. Nếu đã đăng nhập, kiểm tra xem tài liệu này đã có trong danh sách SavedDocuments của user chưa
        isSaved = await _context.SavedDocuments
            .AnyAsync(sd => sd.UserId == userId && sd.DocumentId == id);
    }

    // 3. Truyền biến isSaved ra View bằng ViewBag
    ViewBag.IsSaved = isSaved;

    return View(document);
}
    }
}