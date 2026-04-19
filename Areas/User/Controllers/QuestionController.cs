using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.DTOs.Requests;
using StudyShare.Services.Interfaces;
using System.Security.Claims; // Dùng cái này thay cho UserManager
using AutoMapper; // Dùng AutoMapper để map DTO sang ViewModel, tránh lộ chi tiết DB
using StudyShare.ViewModels; // Bổ sung thư viện này để dùng ViewModel
namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize] // Bắt buộc đăng nhập
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly IMapper _mapper; // Dùng AutoMapper để map DTO sang ViewModel, tránh lộ chi tiết DB
        
        // SỬA: Đã bỏ UserManager, Controller giờ đây cực kỳ nhẹ và sạch
        public QuestionController(IQuestionService questionService, IMapper mapper)
        {
            _questionService = questionService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _questionService.GetAllAsync();
            var viewModel = _mapper.Map<IEnumerable<QuestionViewModel>>(data);
            return View(viewModel);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionCreateRequest request)
        {
            if (!ModelState.IsValid) return View(request);
            
            // SỬA: Lấy currentUserId trực tiếp từ Claims (nhanh và không chọc xuống DB)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            await _questionService.CreateAsync(request, currentUserId);
            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var request = await _questionService.GetForEditAsync(id);
            if (request == null) return NotFound();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionUpdateRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            // SỬA: Lấy currentUserId bằng Claims
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            bool isAdmin = User.IsInRole("Admin");

            var success = await _questionService.UpdateAsync(request, currentUserId, isAdmin);
            if (!success) return Unauthorized("Bạn không có quyền sửa câu hỏi này!");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            // SỬA: Lấy currentUserId bằng Claims
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            bool isAdmin = User.IsInRole("Admin");

            await _questionService.DeleteAsync(id, currentUserId, isAdmin);
            return RedirectToAction(nameof(Index));
        }
    }
}