using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.Services.Interfaces;
using StudyShare.ViewModels;
using  System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly IAnswerService _answerService; // Bổ sung AnswerService
        private readonly IMapper _mapper;

        public QuestionController(IQuestionService questionService, IMapper mapper, IAnswerService answerService) // Bổ sung AnswerService vào constructor
        {
            _questionService = questionService;
            _answerService = answerService; // Gán AnswerService1234
            _mapper = mapper;
        }

public async Task<IActionResult> Index(string searchString)
{
    ViewData["CurrentFilter"] = searchString;

    var questions = await _questionService.GetAllAsync();
    var viewModels = _mapper.Map<IEnumerable<QuestionViewModel>>(questions);

    if (!string.IsNullOrEmpty(searchString))
    {
        searchString = searchString.ToLower();
        viewModels = viewModels.Where(q => 
            (q.AuthorEmail != null && q.AuthorEmail.ToLower().Contains(searchString)) ||
            (q.AuthorName != null && q.AuthorName.ToLower().Contains(searchString)) ||
            (q.Content != null && q.Content.ToLower().Contains(searchString))
        ).ToList();
    }

    return View(viewModels);
}
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Lấy thông tin chi tiết câu hỏi (dùng hàm phù hợp trong IQuestionService)
            var questionResponse = await _questionService.GetByIdAsync(id); 
            
            if (questionResponse == null)
            {
                return NotFound(); // Trả về trang 404 nếu không tìm thấy ID câu hỏi
            }

            // Map dữ liệu sang ViewModel để truyền cho View
            var viewModel = _mapper.Map<QuestionViewModel>(questionResponse);
            
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
           // Lấy ID của Admin đang đăng nhập
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            
            // Truyền đủ 3 tham số: id cần xóa, id người xóa, và cờ isAdmin = true
            var success = await _questionService.DeleteAsync(id, currentUserId, true);
            
            if (success) TempData["Success"] = "Đã xóa toàn bộ câu hỏi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            // Lấy ID của Admin đang đăng nhập
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            
            // Truyền đủ 3 tham số: id cần xóa, id người xóa, và cờ isAdmin = true
            var success = await _answerService.DeleteAsync(id, currentUserId, true); 
            
            if (success) TempData["Success"] = "Đã xóa câu trả lời vi phạm khỏi hệ thống.";
            
            return Redirect(Request.Headers["Referer"].ToString()); // Quay lại trang chi tiết câu hỏi sau khi xóa
        }
    }
}