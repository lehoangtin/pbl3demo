using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.Services.Interfaces;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize] // Bắt buộc đăng nhập
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly UserManager<AppUser> _userManager; // Vẫn giữ UserManager để lấy ID người dùng hiện tại

        public QuestionController(IQuestionService questionService, UserManager<AppUser> userManager)
        {
            _questionService = questionService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _questionService.GetAllAsync();
            return View(data);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionCreateRequest request)
        {
            if (!ModelState.IsValid) return View(request);
            
            var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
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

            var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
            bool isAdmin = User.IsInRole("Admin");

            var success = await _questionService.UpdateAsync(request, currentUserId, isAdmin);
            if (!success) return Unauthorized("Bạn không có quyền sửa câu hỏi này!");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
            bool isAdmin = User.IsInRole("Admin");

            await _questionService.DeleteAsync(id, currentUserId, isAdmin);
            return RedirectToAction(nameof(Index));
        }
    }
}