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

            var dtoList = await _questionService.GetAllAsync();
            var viewModels = _mapper.Map<IEnumerable<QuestionViewModel>>(dtoList);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                viewModels = viewModels.Where(q => 
                    (!string.IsNullOrEmpty(q.Title) && q.Title.ToLower().Contains(searchString)) ||
                    (!string.IsNullOrEmpty(q.Content) && q.Content.ToLower().Contains(searchString)) ||
                    (!string.IsNullOrEmpty(q.AuthorName) && q.AuthorName.ToLower().Contains(searchString))
                );
            }

            return View(viewModels);
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