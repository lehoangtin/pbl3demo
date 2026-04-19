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
    [Authorize(Roles = "Admin")]
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
            await _documentService.ApproveDocumentAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _documentService.DeleteByAdminAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
