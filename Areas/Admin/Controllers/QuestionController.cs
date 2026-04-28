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
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly IMapper _mapper;

        public QuestionController(IQuestionService questionService, IMapper mapper)
        {
            _questionService = questionService;
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
            var success = await _questionService.DeleteByAdminAsync(id);
            if (success) TempData["Success"] = "Đã xóa câu hỏi vi phạm.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            // Lưu ý: ID ở đây là ID của Answer
            var success = await _questionService.DeleteByAdminAsync(id); 
            if (success) TempData["Success"] = "Đã xóa câu trả lời vi phạm.";
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}