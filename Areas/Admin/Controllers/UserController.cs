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
            var usersDto = await _userService.GetAllUsersAsync();
            var viewModels = _mapper.Map<IEnumerable<UserViewModel>>(usersDto);
            return View(viewModels);
        }

        public async Task<IActionResult> Details(string id)
        {
            var userDto = await _userService.GetUserProfileAsync(id);
            if (userDto == null) return NotFound();
            return View(_mapper.Map<UserViewModel>(userDto));
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
public async Task<IActionResult> PendingReports()
{
    var reportsDto = await _reportService.GetAllPendingReportsAsync();
    var viewModels = _mapper.Map<IEnumerable<ReportViewModel>>(reportsDto);
    return View(viewModels);
}

// Thêm Action để Admin bấm nút xử phạt trực tiếp
[HttpPost]
public async Task<IActionResult> Penalize(string userId, int reportId, int pointsDeducted = 10)
{
    // Gọi UserService để trừ điểm và tăng WarningCount
    await _userService.PenalizeUserAsync(userId, pointsDeducted, 1);
    
    // Đánh dấu báo cáo này đã được giải quyết
    await _reportService.ResolveReportAsync(reportId);
    
    TempData["Success"] = "Đã thực hiện xử phạt.";
    return RedirectToAction(nameof(PendingReports));
}
    }
}