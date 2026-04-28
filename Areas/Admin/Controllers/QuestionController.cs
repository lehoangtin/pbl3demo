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

        public async Task<IActionResult> Index()
        {
            var dtoList = await _questionService.GetAllAsync();
            var viewModels = _mapper.Map<IEnumerable<QuestionViewModel>>(dtoList);
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