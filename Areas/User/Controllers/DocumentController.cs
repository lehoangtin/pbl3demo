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
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public DocumentController(IDocumentService documentService, ICategoryService categoryService, IMapper mapper)
        {
            _documentService = documentService;
            _categoryService = categoryService;
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
    }
}