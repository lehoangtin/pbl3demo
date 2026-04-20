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

        public async Task<IActionResult> Index()
        {
            var dtoList = await _questionService.GetAllAsync();
            var viewModels = _mapper.Map<IEnumerable<QuestionViewModel>>(dtoList);
            return View(viewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var questionDto = await _questionService.GetByIdAsync(id);
            if (questionDto == null) return NotFound();

            var viewModel = _mapper.Map<QuestionViewModel>(questionDto);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _questionService.DeleteByAdminAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            await _questionService.DeleteByAdminAsync(id);
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
