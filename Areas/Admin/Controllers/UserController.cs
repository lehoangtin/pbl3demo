using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;
using StudyShare.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IReportService _reportService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IReportService reportService, IMapper mapper)
        {
            _userService = userService;
            _reportService = reportService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            // Map danh sách Entity sang danh sách ViewModel
            var viewModels = _mapper.Map<IEnumerable<UserViewModel>>(users);
            return View(viewModels);
        }

        public async Task<IActionResult> Details(string id)
        {
          var user = await _userService.GetUserProfileAsync(id);
            if (user == null) return NotFound();

            var viewModel = _mapper.Map<UserViewModel>(user);
            return View(viewModel);;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBan(string id)
        {
            var result = await _userService.ToggleBanUserAsync(id);
            // Giả sử service trả về trạng thái mới để báo cho Admin
            TempData["Success"] = "Đã cập nhật trạng thái hoạt động của tài khoản.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ReportedUsers()
        {
            var usersDto = await _userService.GetReportedUsersAsync();
            var viewModels = _mapper.Map<IEnumerable<UserViewModel>>(usersDto);
            return View(viewModels);
        }

        public async Task<IActionResult> ViewReports(string userId)
        {
            var reportsDto = await _reportService.GetReportsForUserAsync(userId);
            var viewModels = _mapper.Map<IEnumerable<ReportViewModel>>(reportsDto);
            ViewBag.TargetUserId = userId;
            return View(viewModels);
        }
        // Trong Areas/Admin/Controllers/UserController.cs
// Trang này sẽ liệt kê MỌI báo cáo mới nhất từ MỌI người dùng
// 1. Trang danh sách tập trung mọi báo cáo
public async Task<IActionResult> PendingReports()
{
    var reportsDto = await _reportService.GetAllPendingReportsAsync();
    var viewModels = _mapper.Map<IEnumerable<ReportViewModel>>(reportsDto);
    return View(viewModels);
}

// 2. Action Xử phạt: Chỉ chạy khi Admin xác nhận
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Penalize(string userId, int reportId, int pointsDeducted = 10)
{
    // Thực hiện trừ điểm và tăng WarningCount qua UserService
    var success = await _userService.PenalizeUserAsync(userId, pointsDeducted, 1);
    
    if (success)
    {
        // Cập nhật trạng thái báo cáo là đã giải quyết
        await _reportService.ResolveWithActionAsync(reportId, $"Admin đã phạt trừ {pointsDeducted} điểm.");
        TempData["Success"] = "Đã xử phạt và trừ điểm thành công.";
    }
    
    return RedirectToAction(nameof(PendingReports));
}

// 3. Action Bỏ qua: Nếu báo cáo sai, không phạt
[HttpPost]
public async Task<IActionResult> DismissReport(int reportId)
{
    await _reportService.ResolveWithActionAsync(reportId, "Admin đã bỏ qua báo cáo này.");
    return RedirectToAction(nameof(PendingReports));
}
    }
}