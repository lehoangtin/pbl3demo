using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyShare.Models;
using StudyShare.ViewModels;
using StudyShare.Services.Interfaces;
using System.Threading.Tasks;
using System.Security.Claims; 
using AutoMapper;
using System.Collections.Generic; // Bổ sung để dùng IEnumerable

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
        private readonly IReportService _reportService;
        private readonly IMapper _mapper;

        public UserController(
            IUserService userService,
            IDocumentService documentService,
            IQuestionService questionService,
            IAnswerService answerService,
            IReportService reportService,
            UserManager<AppUser> userManager,
            IMapper mapper)
        {
            _userService = userService;
            _documentService = documentService;
            _questionService = questionService;
            _answerService = answerService;
            _reportService = reportService;
            _userManager = userManager;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return RedirectToAction("Profile", new { id = userId });
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            // 1. Lấy DTO từ Service
            var userDto = await _userService.GetUserProfileAsync(userId);

            if (userDto == null) return NotFound();

            // Gán ViewBag lấy số lượng (dùng DTO)
            ViewBag.TotalDocs = userDto.Documents?.Count ?? 0;
            ViewBag.TotalQuestions = userDto.Questions?.Count ?? 0;
            ViewBag.TotalSaved = userDto.SavedDocuments?.Count ?? 0;

            // 2. Map sang ViewModel và trả ra View
            var viewModel = _mapper.Map<UserViewModel>(userDto);
            return View(viewModel);
        }

        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId); 
            return View(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(AppUser model, IFormFile? avatarFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            ViewBag.CurrentUser = await _userManager.FindByIdAsync(userId);
            
            // SỬA Ở ĐÂY: Map DTO sang ViewModel
            var dtoList = await _questionService.GetUserQuestionsAsync(userId); 
            var viewModels = _mapper.Map<IEnumerable<QuestionViewModel>>(dtoList);
            
            return View(viewModels);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var success = await _documentService.DeleteByUserAsync(id, userId); 
            if (success) TempData["Success"] = "Tài liệu của bạn đã được xóa vĩnh viễn.";
            
            return RedirectToAction(nameof(MyDocuments));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var success = await _questionService.DeleteByUserAsync(id, userId);
            if (success) TempData["Success"] = "Đã xóa thảo luận và các dữ liệu liên quan thành công.";
            
            return RedirectToAction(nameof(MyQuestions));
        }

        public async Task<IActionResult> MyDocuments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            ViewBag.CurrentUser = await _userManager.FindByIdAsync(userId);
            
            // SỬA Ở ĐÂY: Map DTO sang ViewModel
            var dtoList = await _documentService.GetUserDocumentsAsync(userId);
            var viewModels = _mapper.Map<IEnumerable<DocumentViewModel>>(dtoList);
            
            return View(viewModels);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocument(int docId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var success = await _userService.SaveDocumentAsync(userId, docId);
            if (success) TempData["Success"] = "Đã lưu tài liệu vào danh sách của bạn!";
            
            return RedirectToAction("ViewDocument", "Home", new { area = "", id = docId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnsaveDocument(int docId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            ViewBag.CurrentUser = await _userManager.FindByIdAsync(userId);
            
            // SỬA Ở ĐÂY: Map DTO sang ViewModel
            var dtoList = await _userService.GetSavedDocumentsAsync(userId);
            var viewModels = _mapper.Map<IEnumerable<DocumentViewModel>>(dtoList);
            
            return View(viewModels);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var success = await _answerService.DeleteByUserAsync(id, userId);

            if (!success) TempData["Error"] = "Bạn không có quyền xoá hoặc nội dung không tồn tại.";
            else TempData["Success"] = "Đã xoá câu trả lời.";

            string returnUrl = Request.Headers["Referer"].ToString();
            return string.IsNullOrEmpty(returnUrl) ? RedirectToAction("Index", "Home") : Redirect(returnUrl);
        }
        public async Task<IActionResult> ViewReports(string userId)
        {
            // 1. Lấy DTO từ Service
            var reportsDto = await _reportService.GetReportsForUserAsync(userId);
            
            // 2. Map sang ViewModel để hiển thị ra giao diện
            var viewModels = _mapper.Map<IEnumerable<ReportViewModel>>(reportsDto);
            
            ViewBag.TargetUserId = userId;
            return View(viewModels);
        }

    }
}
