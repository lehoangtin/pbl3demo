using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudyShare.DTOs.Requests;
using StudyShare.Services.Interfaces;
using StudyShare.ViewModels;
using System.Security.Claims;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly IUserService _userService; // 🔥 Đã khai báo UserService
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment; // 🔥 Đã khai báo IWebHostEnvironment
        private readonly IMapper _mapper;

        public DocumentController(IDocumentService documentService, IUserService userService, ICategoryService categoryService, IWebHostEnvironment webHostEnvironment, IMapper mapper)
        {
            _documentService = documentService;
            _userService = userService;
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. Lấy ID của người dùng đang đăng nhập hiện tại
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            // 2. CHÌA KHÓA Ở ĐÂY: Thay vì dùng GetAllAsync(), 
            // chúng ta dùng GetUserDocumentsAsync để chỉ lấy đúng đồ của mình.
            var dtoList = await _documentService.GetUserDocumentsAsync(currentUserId);
            
            // 3. Map sang ViewModel để hiển thị lên View
            var viewModels = _mapper.Map<IEnumerable<DocumentViewModel>>(dtoList);

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var data = await _documentService.GetDocumentDetailsAsync(id);
            if (data == null) return NotFound();
            
            var viewModel = _mapper.Map<DocumentViewModel>(data);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllAsync();
                ViewBag.CategoryId = new SelectList(categories, "Id", "Name", request.CategoryId);
                return View(request);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            await _documentService.CreateAsync(request, currentUserId);
            await _userService.AddPointsAsync(currentUserId, 10);
            TempData["Success"] = "Tải lên thành công! Tài liệu đang chờ duyệt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var request = await _documentService.GetForEditAsync(id);
            if (request == null) return NotFound();

            var categories = await _categoryService.GetAllAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", request.CategoryId);

            var existingDocument = await _documentService.GetByIdAsync(id);
            ViewBag.CurrentFileName = existingDocument?.FileName ?? "Không có tệp";

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DocumentUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllAsync();
                ViewBag.CategoryId = new SelectList(categories, "Id", "Name", request.CategoryId);
                var existingDocument = await _documentService.GetByIdAsync(request.Id);
                ViewBag.CurrentFileName = existingDocument?.FileName ?? "Không có tệp";
                return View(request);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            bool isAdmin = User.IsInRole("Admin");

            var success = await _documentService.UpdateAsync(request, currentUserId, isAdmin);
            if (!success) return Unauthorized("Bạn không có quyền chỉnh sửa tài liệu này hoặc tài liệu không tồn tại.");

            TempData["Success"] = "Cập nhật tài liệu thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Đã thêm HttpPost và ValidateAntiForgeryToken để bảo mật việc xóa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            bool isAdmin = User.IsInRole("Admin");

            var success = await _documentService.DeleteAsync(id, currentUserId, isAdmin);
            if (success)
            {
                TempData["Success"] = "Xóa tài liệu thành công!";
            }
            else
            {
                TempData["Error"] = "Không thể xóa tài liệu. Bạn không có quyền hoặc tài liệu không tồn tại.";
            }
            
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            // Fix lỗi _userManager: Dùng ClaimTypes để lấy UserId trực tiếp từ User
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Challenge();

            // 1. Fix lỗi GetDocumentByIdAsync và GetUserByIdAsync: Sử dụng đúng tên hàm trong Interface
            var document = await _documentService.GetByIdAsync(id);
            var user = await _userService.GetUserProfileAsync(currentUserId);

            if (document == null || user == null) return NotFound("Không tìm thấy tài liệu hoặc người dùng.");

            // 2. Kiểm tra điều kiện tải (Tốn 10 điểm)
            int downloadCost = 10;
            
            // Fix lỗi Null Reference: Dùng document.UserId so sánh với currentUserId thay vì User.Identity.Name
            bool isFree = User.IsInRole("Admin") || document.UserId == currentUserId;

            if (!isFree)
            {
                // Lưu ý: Đảm bảo thuộc tính điểm trong model AppUser của bạn là Point hoặc Points
                if (user.Points < downloadCost) 
                {
                    TempData["Error"] = $"Bạn không đủ điểm để tải tài liệu này (Cần {downloadCost} điểm, bạn đang có {user.Points} điểm).";
                    return RedirectToAction("Details", new { id = id });
                }

                // 3. Fix lỗi UpdateUserAsync: Sử dụng đúng hàm PenalizeUserAsync có sẵn để trừ điểm
                await _userService.PenalizeUserAsync(currentUserId, downloadCost, 0);
            }

            // 4. Fix lỗi UpdateDocumentAsync: Sử dụng đúng hàm IncreaseDownloadCountAsync có sẵn
            await _documentService.IncreaseDownloadCountAsync(id);

            // 5. Trả về file vật lý
            // Dùng TrimStart('/') để tránh lỗi ghép sai đường dẫn Path.Combine
            var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, document.FilePath.TrimStart('/'));
            
            if (!System.IO.File.Exists(physicalPath))
            {
                TempData["Error"] = "Tệp tin không tồn tại trên hệ thống.";
                return RedirectToAction("Details", new { id = id });
            }

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
            
            // Nếu document.FileType rỗng thì dùng octet-stream làm mặc định
            string fileType = !string.IsNullOrEmpty(document.FileType) ? document.FileType : "application/octet-stream";
            return File(fileBytes, fileType, document.FileName);
        }
    }
}