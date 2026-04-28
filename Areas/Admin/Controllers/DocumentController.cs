using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.Services.Interfaces;
using StudyShare.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly IMapper _mapper;

        public DocumentController(IDocumentService documentService, IMapper mapper)
        {
            _documentService = documentService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var dtoList = await _documentService.GetAllAsync();
            var viewModels = _mapper.Map<IEnumerable<DocumentViewModel>>(dtoList);
            return View(viewModels);
        }

        public async Task<IActionResult> Review(int id)
        {
            var docDto = await _documentService.GetByIdAsync(id);
            if (docDto == null) return NotFound();
            
            var viewModel = _mapper.Map<DocumentViewModel>(docDto);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var success = await _documentService.ApproveDocumentAsync(id);
            if (success) TempData["Success"] = "Đã phê duyệt tài liệu thành công!";
            else TempData["Error"] = "Có lỗi xảy ra khi phê duyệt.";
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _documentService.DeleteByAdminAsync(id);
            if (success) TempData["Success"] = "Đã xóa tài liệu khỏi hệ thống.";
            else TempData["Error"] = "Không thể xóa tài liệu này.";
            
            return RedirectToAction(nameof(Index));
        }
    }
}