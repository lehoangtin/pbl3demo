using AutoMapper; // 1. Bổ sung thư viện này
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudyShare.DTOs.Requests;
using StudyShare.Services.Interfaces;
using StudyShare.ViewModels; // 2. Bổ sung thư viện này
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

        public async Task<IActionResult> Index()
        {
            var data = await _documentService.GetAllAsync();
            var viewModel = _mapper.Map<IEnumerable<DocumentViewModel>>(data);
            return View(viewModel);
        }

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

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            bool isAdmin = User.IsInRole("Admin");

            var success = await _documentService.UpdateAsync(request, currentUserId, isAdmin);
            if (!success) return Unauthorized("Bạn không có quyền chỉnh sửa tài liệu này hoặc tài liệu không tồn tại.");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            bool isAdmin = User.IsInRole("Admin");

            await _documentService.DeleteAsync(id, currentUserId, isAdmin);
            return RedirectToAction(nameof(Index));
        }
    }
}