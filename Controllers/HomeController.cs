using Microsoft.AspNetCore.Mvc;
using StudyShare.Models;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Identity; 
using System.Threading.Tasks;
using StudyShare.Services.Interfaces; // Thêm thư viện gọi Service

namespace StudyShare.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        // Chỉ tiêm các Service và Manager, KHÔNG tiêm AppDbContext
        public HomeController(
            IDocumentService documentService,
            ICategoryService categoryService,
            IUserService userService,
            SignInManager<AppUser> signInManager, 
            UserManager<AppUser> userManager)
        {
            _documentService = documentService;
            _categoryService = categoryService;
            _userService = userService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchTerm, int? categoryId)
        {
            var user = await _userManager.GetUserAsync(User);
            // Kiểm tra ban qua Service
            if (user != null && await _userService.IsUserBannedAsync(user.Id))
            {
                await _signInManager.SignOutAsync();
                TempData["Error"] = "Tài khoản của bạn đã bị khóa!";
                return RedirectToAction("Login", "Account", new { area = "" }); 
            }

            // Gọi Service để lấy danh sách
            var docs = await _documentService.GetApprovedDocumentsAsync(searchTerm, categoryId);
            
            ViewBag.SearchTerm = searchTerm; 
            ViewBag.CurrentCategory = categoryId; 
            // Gọi CategoryService để lấy danh mục hiển thị menu
            ViewBag.Categories = await _categoryService.GetAllAsync(); 

            return View(docs);
        }

        [Authorize] 
        public async Task<IActionResult> ViewDocument(int id)
        {
            // Gọi Service để lấy chi tiết
            var document = await _documentService.GetDocumentDetailsAsync(id);
            if (document == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            
            // Gọi Service để kiểm tra đã lưu chưa
            bool isSaved = false;
            if (userId != null)
            {
                isSaved = await _userService.IsDocumentSavedAsync(userId, id);
            }

            ViewBag.IsSaved = isSaved;

            return View(document);
        }
    }
}