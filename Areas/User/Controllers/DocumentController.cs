using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.Services.Interfaces;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly ICategoryService _categoryService; // Gọi CategoryService để lấy danh sách thẻ Select
        private readonly UserManager<AppUser> _userManager;

        public DocumentController(IDocumentService documentService, ICategoryService categoryService, UserManager<AppUser> userManager)
        {
            _documentService = documentService;
            _categoryService = categoryService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _documentService.GetAllAsync();
            return View(data);
        }

        public async Task<IActionResult> Create()
        {
            // Truyền danh sách Category ra View để làm Dropdown
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
            
            var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
            await _documentService.CreateAsync(request, currentUserId);
            
            return RedirectToAction(nameof(Index));
        }

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

            var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
            bool isAdmin = User.IsInRole("Admin");

            var success = await _documentService.UpdateAsync(request, currentUserId, isAdmin);
            if (!success) return Unauthorized("Bạn không có quyền chỉnh sửa tài liệu này hoặc tài liệu không tồn tại.");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
            bool isAdmin = User.IsInRole("Admin");

            await _documentService.DeleteAsync(id, currentUserId, isAdmin);
            return RedirectToAction(nameof(Index));
        }
    }
}