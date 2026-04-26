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
        public async Task<IActionResult> Reports()
        {
            var pendingReportsDto = await _reportService.GetAllPendingReportsAsync(); 
            var resolvedReportsDto = await _reportService.GetResolvedReportsAsync();

            var viewModel = new ReportDashboardViewModel
            {
                // Đã thêm _mapper.Map ở đây để fix lỗi CS0266
                PendingReports = _mapper.Map<IEnumerable<ReportViewModel>>(pendingReportsDto),
                ResolvedReports = _mapper.Map<IEnumerable<ReportViewModel>>(resolvedReportsDto)
            };

            return View(viewModel);
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
    
    // Lấy thêm danh sách vi phạm của người dùng này
    var reportsDto = await _reportService.GetReportsForUserAsync(id);
    ViewBag.Violations = _mapper.Map<IEnumerable<ReportViewModel>>(reportsDto);

    return View(viewModel);
}
        [HttpPost]
        public async Task<IActionResult> ToggleBan(string id)
        {
            var result = await _userService.ToggleBanUserAsync(id);
            // Giả sử service trả về trạng thái mới để báo cho Admin
            TempData["Success"] = "Đã cập nhật trạng thái hoạt động của tài khoản.";
            return RedirectToAction(nameof(Index));
        }
    
// 2. Action Xử phạt: Chỉ chạy khi Admin xác nhận
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Penalize(string userId, int reportId, int pointsDeducted = 10)
{
    var success = await _userService.PenalizeUserAsync(userId, pointsDeducted, 1);
    
    if (success)
    {
        await _reportService.ResolveWithActionAsync(reportId, $"Admin đã phạt trừ {pointsDeducted} điểm.");
        TempData["Success"] = "Đã xử phạt và trừ điểm thành công.";
    }
    
    // ĐÃ SỬA: Chuyển hướng về trang Reports mới
    return RedirectToAction(nameof(Reports));
}

// 3. Action Bỏ qua: Nếu báo cáo sai, không phạt
[HttpPost]
public async Task<IActionResult> DismissReport(int reportId)
{
    await _reportService.ResolveWithActionAsync(reportId, "Admin đã bỏ qua báo cáo này.");
        return RedirectToAction(nameof(Reports));
}
    }
}