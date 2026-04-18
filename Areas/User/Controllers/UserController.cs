using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyShare.Models;
using StudyShare.ViewModels;
using StudyShare.Services.Interfaces;
using System.Threading.Tasks;

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IDocumentService _documentService;
        private readonly IQuestionService _questionService;
        private readonly IAnswerService _answerService;
        private readonly UserManager<AppUser> _userManager;

        // Tiêm toàn bộ các Service vào thay cho AppDbContext
        public UserController(
            IUserService userService,
            IDocumentService documentService,
            IQuestionService questionService,
            IAnswerService answerService,
            UserManager<AppUser> userManager)
        {
            _userService = userService;
            _documentService = documentService;
            _questionService = questionService;
            _answerService = answerService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            return RedirectToAction("Profile", new { id = userId });
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = await _userService.GetUserProfileAsync(userId); // Gọi qua Service

            if (user == null) return NotFound();

            ViewBag.TotalDocs = user.Documents?.Count ?? 0;
            ViewBag.TotalQuestions = user.Questions?.Count ?? 0;
            ViewBag.TotalSaved = user.SavedDocuments?.Count ?? 0;

            return View(user);
        }

        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId); 
            return View(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(AppUser model, IFormFile? avatarFile)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            // Service sẽ lo việc đổi tên và lưu file ảnh vào wwwroot
            await _userService.UpdateUserProfileAsync(userId, model, avatarFile);
            return RedirectToAction("Profile", new { id = userId });
        }

        [Authorize]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
                return View(model);
            }

            return RedirectToAction("Profile", new { id = user.Id });
        }

        public async Task<IActionResult> MyQuestions()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            ViewBag.CurrentUser = await _userManager.FindByIdAsync(userId);
            var questions = await _questionService.GetUserQuestionsAsync(userId); // Gọi qua Service
            return View(questions);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            // Service sẽ lo logic xóa Report, SavedDocs và File vật lý
            var success = await _documentService.DeleteByUserAsync(id, userId); 
            if (success) TempData["Success"] = "Tài liệu của bạn đã được xóa vĩnh viễn.";
            
            return RedirectToAction(nameof(MyDocuments));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var userId = _userManager.GetUserId(User);
            // Service sẽ lo logic xóa Report của Question và Answer
            var success = await _questionService.DeleteByUserAsync(id, userId);
            if (success) TempData["Success"] = "Đã xóa thảo luận và các dữ liệu liên quan thành công.";
            
            return RedirectToAction(nameof(MyQuestions));
        }

        public async Task<IActionResult> MyDocuments()
        {
            var userId = _userManager.GetUserId(User);            if (string.IsNullOrEmpty(userId)) return Unauthorized();            ViewBag.CurrentUser = await _userManager.FindByIdAsync(userId);
            var docs = await _documentService.GetUserDocumentsAsync(userId);
            return View(docs);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocument(int docId)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _userService.SaveDocumentAsync(userId, docId);
            if (success) TempData["Success"] = "Đã lưu tài liệu vào danh sách của bạn!";
            
            return RedirectToAction("ViewDocument", "Home", new { area = "", id = docId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnsaveDocument(int docId)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _userService.UnsaveDocumentAsync(userId, docId);
            if (success) TempData["Success"] = "Đã bỏ lưu tài liệu.";

            string returnUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("SavedDocuments"))
            {
                return RedirectToAction(nameof(SavedDocuments));
            }
            return RedirectToAction("ViewDocument", "Home", new { area = "", id = docId });
        }

        public async Task<IActionResult> SavedDocuments()
        {
            var userId = _userManager.GetUserId(User);            if (string.IsNullOrEmpty(userId)) return Unauthorized();            ViewBag.CurrentUser = await _userManager.FindByIdAsync(userId);
            var savedDocs = await _userService.GetSavedDocumentsAsync(userId);
            return View(savedDocs);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var success = await _answerService.DeleteByUserAsync(id, userId);

            if (!success) TempData["Error"] = "Bạn không có quyền xoá hoặc nội dung không tồn tại.";
            else TempData["Success"] = "Đã xoá câu trả lời.";

            string returnUrl = Request.Headers["Referer"].ToString();
            return string.IsNullOrEmpty(returnUrl) ? RedirectToAction("Index", "Home") : Redirect(returnUrl);
        }
    }
}