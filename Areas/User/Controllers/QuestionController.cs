using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.DTOs.Requests;
using StudyShare.Services.Interfaces;
using System.Security.Claims;
using AutoMapper;
using StudyShare.ViewModels;
using StudyShare.Models;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly IAnswerService _answerService; 
        private readonly IMapper _mapper;
        private readonly IAIService _aiService; 
        private readonly IUserService _userService;
        
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public QuestionController(
            IQuestionService questionService, 
            IAnswerService answerService, 
            IMapper mapper, 
            IAIService aiService, 
            IUserService userService,
            AppDbContext context,
            UserManager<AppUser> userManager)
        {
            _questionService = questionService;
            _answerService = answerService;
            _mapper = mapper;
            _aiService = aiService;
            _userService = userService;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchString)
{
    // Lưu lại từ khóa tìm kiếm để hiển thị lại trên ô input sau khi tải trang
    ViewData["CurrentFilter"] = searchString;

    var questionsDto = await _questionService.GetAllAsync();
    var viewModels = _mapper.Map<IEnumerable<QuestionViewModel>>(questionsDto);

    // Thực hiện lọc nếu có từ khóa
    if (!string.IsNullOrEmpty(searchString))
    {
        searchString = searchString.ToLower();
        viewModels = viewModels.Where(q => 
            (q.Title != null && q.Title.ToLower().Contains(searchString)) || 
            (q.Content != null && q.Content.ToLower().Contains(searchString))
        );
    }

    // Sắp xếp câu hỏi mới nhất lên đầu
    viewModels = viewModels.OrderByDescending(q => q.CreatedAt);

    return View(viewModels);
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
        public async Task<IActionResult> Create(QuestionCreateViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);
            
            var request = _mapper.Map<QuestionCreateRequest>(viewModel);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            // KIỂM DUYỆT AI
            var aiCheck = await _aiService.CheckContentAsync(request.Content);
            if (aiCheck.isFlagged)
            {
                // Xử phạt user
                await _userService.PenalizeUserAsync(currentUserId, 10, 1);

                // 🔥 LƯU REPORT VÀ ĐÁNH DẤU LÀ ĐÃ GIẢI QUYẾT
                var autoReport = new Report
                {
                    ReporterUserId = currentUserId, 
                    TargetUserId = currentUserId,
                    Reason = $"[HỆ THỐNG AI CHẶN TỰ ĐỘNG] Lý do: {aiCheck.reason}. Nội dung gốc: {request.Content}",
                    IsResolved = true, // Tự động đưa vào mục đã giải quyết
                    ActionTaken = "Hệ thống AI đã tự động chặn và xử phạt (Trừ 10 điểm, 1 gậy cảnh cáo)."
                };
                _context.Reports.Add(autoReport);
                await _context.SaveChangesAsync();

                TempData["Error"] = $"Nội dung vi phạm: {aiCheck.reason}. Bạn bị trừ 10 điểm và nhận 1 gậy cảnh cáo.";
                return View(viewModel);
            }

            // TẠO CÂU HỎI
            await _questionService.CreateAsync(request, currentUserId);

            // CỘNG ĐIỂM
            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user != null) 
            {
                user.Points += 5; 
                await _userManager.UpdateAsync(user);
            }

            TempData["Success"] = "Đăng câu hỏi thành công! Bạn được cộng 5 điểm.";
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

            var viewModel = new QuestionEditViewModel { Id = question.Id, Content = question.Content };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionEditViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            var request = _mapper.Map<QuestionUpdateRequest>(viewModel);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var aiCheck = await _aiService.CheckContentAsync(request.Content);
            if (aiCheck.isFlagged)
            {
                await _userService.PenalizeUserAsync(currentUserId, 10, 1);

                // 🔥 LƯU REPORT VÀ ĐÁNH DẤU LÀ ĐÃ GIẢI QUYẾT
                var autoReport = new Report
                {
                    ReporterUserId = currentUserId,
                    TargetUserId = currentUserId,
                    QuestionId = request.Id,
                    Reason = $"[HỆ THỐNG AI CHẶN SỬA] Lý do: {aiCheck.reason}. Nội dung vi phạm: {request.Content}",
                    IsResolved = true, // Tự động đưa vào mục đã giải quyết
                    ActionTaken = "Hệ thống AI đã tự động chặn và xử phạt (Trừ 10 điểm, 1 gậy cảnh cáo)."
                };
                _context.Reports.Add(autoReport);
                await _context.SaveChangesAsync();

                TempData["Error"] = $"Vi phạm khi sửa: {aiCheck.reason}. Bạn bị trừ 10 điểm và nhận 1 gậy cảnh cáo.";
                return View(viewModel);
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
        public async Task<IActionResult> PostAnswer(AnswerCreateViewModel viewModel)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Details), new { id = viewModel.QuestionId });

            var request = _mapper.Map<AnswerCreateRequest>(viewModel);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            // KIỂM DUYỆT AI CHO CÂU TRẢ LỜI
            var aiCheck = await _aiService.CheckContentAsync(request.Content);
            if (aiCheck.isFlagged)
            {
                await _userService.PenalizeUserAsync(currentUserId, 10, 1);

                // 🔥 LƯU REPORT VÀ ĐÁNH DẤU LÀ ĐÃ GIẢI QUYẾT
                var autoReport = new Report
                {
                    ReporterUserId = currentUserId,
                    TargetUserId = currentUserId,
                    QuestionId = request.QuestionId,
                    Reason = $"[HỆ THỐNG AI CHẶN TRẢ LỜI] Lý do: {aiCheck.reason}. Nội dung vi phạm: {request.Content}",
                    IsResolved = true, // Tự động đưa vào mục đã giải quyết
                    ActionTaken = "Hệ thống AI đã tự động chặn và xử phạt (Trừ 10 điểm, 1 gậy cảnh cáo)."
                };
                _context.Reports.Add(autoReport);
                await _context.SaveChangesAsync();

                TempData["Error"] = $"Bình luận vi phạm: {aiCheck.reason}. Bạn bị trừ 10 điểm.";
                return RedirectToAction(nameof(Details), new { id = request.QuestionId });
            }

            var success = await _answerService.CreateAsync(request, currentUserId);
            if (success) 
            {
                var user = await _userManager.FindByIdAsync(currentUserId);
                if (user != null) 
                {
                    user.Points += 3;
                    await _userManager.UpdateAsync(user);
                }
                TempData["Success"] = "Đã đăng câu trả lời! Bạn được cộng 3 điểm."; 
            }
            
            return RedirectToAction(nameof(Details), new { id = request.QuestionId });
        }
        [HttpPost]
[ValidateAntiForgeryToken]
// Cần truyền vào 2 tham số: ID của câu trả lời muốn xóa, và ID của câu hỏi để quay về
public async Task<IActionResult> DeleteAnswer(int answerId, int questionId)
{
    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    bool isAdmin = User.IsInRole("Admin");

    // Tận dụng hàm DeleteAsync trong AnswerService (nó đã tự check quyền chủ sở hữu hoặc Admin)
    var success = await _answerService.DeleteAsync(answerId, currentUserId, isAdmin);
    
    if (success)
    {
        TempData["Success"] = "Đã xóa câu trả lời thành công.";
    }
    else
    {
        TempData["Error"] = "Bạn không có quyền xóa câu trả lời này hoặc có lỗi xảy ra.";
    }

    // Xóa xong thì chuyển hướng người dùng về lại đúng trang chi tiết của Câu hỏi đó
    return RedirectToAction(nameof(Details), new { id = questionId });
}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(int? questionId, int? answerId, string reason)
        {
            var reporterId = _userManager.GetUserId(User);
            string targetUserId = "";

            if (answerId.HasValue)
            {
                var answer = await _context.Answers.FindAsync(answerId);
                if (answer != null) targetUserId = answer.UserId;
            }
            else if (questionId.HasValue)
            {
                var question = await _context.Questions.FindAsync(questionId);
                if (question != null) targetUserId = question.UserId;
            }

            if (string.IsNullOrEmpty(targetUserId) || reporterId == targetUserId)
            {
                TempData["Error"] = "Thao tác không hợp lệ.";
                int returnId = questionId ?? (answerId.HasValue ? _context.Answers.Find(answerId)?.QuestionId ?? 0 : 0);
                return RedirectToAction("Details", new { id = returnId });
            }

            var report = new Report
            {
                ReporterUserId = reporterId,
                TargetUserId = targetUserId,
                QuestionId = questionId,
                AnswerId = answerId,
                Reason = reason
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cảm ơn bạn! Báo cáo đã được gửi tới Quản trị viên.";
            
            int redirectId = questionId ?? (await _context.Answers.FindAsync(answerId)).QuestionId;
            return RedirectToAction("Details", new { id = redirectId });
        }
    }
}