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
            var data = await _documentService.GetAllAsync();
            var viewModel = _mapper.Map<IEnumerable<DocumentViewModel>>(data);
            return View(viewModel);
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
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Challenge();

            // 1. Lấy thông tin tài liệu và người dùng
            var document = await _documentService.GetByIdAsync(id);
            var user = await _userService.GetUserProfileAsync(currentUserId);

            if (document == null) return NotFound();

            // 2. Kiểm tra điều kiện tải (Ví dụ: tốn 10 điểm)
            int downloadCost = 10;

            // Admin hoặc chủ sở hữu tài liệu thì được miễn phí
            bool isFree = User.IsInRole("Admin") || document.UserId == currentUserId;

            if (!isFree)
            {
                if (user.Points < downloadCost)
                {
                    TempData["Error"] = $"Bạn không đủ điểm để tải tài liệu này (Cần {downloadCost} điểm).";
                    return RedirectToAction("Details", new { id = id });
                }

                // 3. Trừ điểm người tải
                await _userService.PenalizeUserAsync(currentUserId, downloadCost, 0);
            }

            // 4. Tăng lượt tải trong DB
            await _documentService.IncreaseDownloadCountAsync(id);

            // 5. Trả về file vật lý
            var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, document.FilePath.TrimStart('/'));
            
            if (!System.IO.File.Exists(physicalPath))
                return NotFound("Tệp tin không tồn tại trên hệ thống.");

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
            return File(fileBytes, document.FileType, document.FileName);
        }
    }
}