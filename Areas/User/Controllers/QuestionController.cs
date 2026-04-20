using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.DTOs.Requests;
using StudyShare.Services.Interfaces;
using System.Security.Claims;
using AutoMapper;
using StudyShare.ViewModels;
using ai.Services; 

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly IAnswerService _answerService; 
        private readonly IMapper _mapper;
        private readonly AIService _aiService; 
        private readonly IUserService _userService; // 🔥 Đã khai báo UserService

        // 🔥 Đã bổ sung IUserService vào Constructor
        public QuestionController(
            IQuestionService questionService, 
            IAnswerService answerService, 
            IMapper mapper, 
            AIService aiService, 
            IUserService userService)
        {
            _questionService = questionService;
            _answerService = answerService;
            _mapper = mapper;
            _aiService = aiService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _questionService.GetAllAsync();
            var viewModel = _mapper.Map<IEnumerable<QuestionViewModel>>(data);
            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var questionDto = await _questionService.GetByIdAsync(id);
            if (questionDto == null) return NotFound();

            var viewModel = _mapper.Map<QuestionViewModel>(questionDto);
            var answers = await _answerService.GetByQuestionIdAsync(id);
            viewModel.Answers = _mapper.Map<IEnumerable<AnswerViewModel>>(answers).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionCreateRequest request)
        {
            if (!ModelState.IsValid) return View(request);
            
            // 🔥 Đưa dòng lấy ID lên trước khi sử dụng để tránh lỗi CS0841
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var aiCheck = await _aiService.CheckContentAsync(request.Content);
            if (aiCheck.isFlagged)
            {
                // PHẠT: Trừ 10 điểm, Tăng 1 lần cảnh báo (WarningCount)
                await _userService.PenalizeUserAsync(currentUserId, 10, 1);
                
                TempData["Error"] = $"Nội dung vi phạm: {aiCheck.reason}. Bạn bị trừ 10 điểm và nhận 1 gậy cảnh cáo.";
                return View(request);
            }

            await _questionService.CreateAsync(request, currentUserId);
            TempData["Success"] = "Đăng câu hỏi thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var question = await _questionService.GetByIdAsync(id);
            if (question == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (question.UserId != currentUserId && !User.IsInRole("Admin"))
                return Unauthorized("Bạn không có quyền sửa câu hỏi này.");

            var request = new QuestionUpdateRequest { Id = question.Id, Content = question.Content };
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionUpdateRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var aiCheck = await _aiService.CheckContentAsync(request.Content);
            if (aiCheck.isFlagged)
            {
                await _userService.PenalizeUserAsync(currentUserId, 10, 1);
                TempData["Error"] = $"Vi phạm khi sửa: {aiCheck.reason}. Bạn bị trừ 10 điểm và nhận 1 gậy cảnh cáo.";
                return View(request);
            }

            bool isAdmin = User.IsInRole("Admin");
            var success = await _questionService.UpdateAsync(request, currentUserId, isAdmin);
            if (!success) return Unauthorized();

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction(nameof(Details), new { id = request.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            bool isAdmin = User.IsInRole("Admin");

            await _questionService.DeleteAsync(id, currentUserId, isAdmin);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostAnswer(AnswerCreateRequest request)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Details), new { id = request.QuestionId });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var aiCheck = await _aiService.CheckContentAsync(request.Content);
            if (aiCheck.isFlagged)
            {
                await _userService.PenalizeUserAsync(currentUserId, 10, 1);
                TempData["Error"] = $"Bình luận vi phạm: {aiCheck.reason}. Bạn bị trừ 10 điểm và nhận 1 gậy cảnh cáo.";
                return RedirectToAction(nameof(Details), new { id = request.QuestionId });
            }

            await _answerService.CreateAsync(request, currentUserId);
            return RedirectToAction(nameof(Details), new { id = request.QuestionId });
        }
    }
}