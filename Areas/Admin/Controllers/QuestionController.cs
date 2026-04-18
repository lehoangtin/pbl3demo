using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.Services.Interfaces;
using System.Threading.Tasks;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;

        // Chỉ tiêm Service, bỏ AppDbContext và UserManager
        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        // 1. Danh sách tất cả câu hỏi
        public async Task<IActionResult> Index()
        {
            var questions = await _questionService.GetAllForAdminAsync();
            return View(questions);
        }

        // 2. Xem chi tiết câu hỏi và các báo cáo liên quan
        public async Task<IActionResult> Details(int id)
        {
            var question = await _questionService.GetDetailsForAdminAsync(id);
            if (question == null) return NotFound();

            // Lấy danh sách báo cáo qua Service
            ViewBag.Reports = await _questionService.GetReportsForQuestionAsync(id);

            return View(question);
        }

        // 3. Xóa câu hỏi (Xóa luôn các câu trả lời/báo cáo liên quan)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            await _questionService.DeleteQuestionByAdminAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // 4. Xóa câu trả lời vi phạm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnswer(int id, int questionId)
        {
            await _questionService.DeleteAnswerByAdminAsync(id);
            return RedirectToAction(nameof(Details), new { id = questionId });
        }
    }
}