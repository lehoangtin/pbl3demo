using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;
using StudyShare.ViewModels;
using System.Linq;
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

        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var users = await _userService.GetAllUsersAsync();
            var viewModels = _mapper.Map<IEnumerable<UserViewModel>>(users);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                viewModels = viewModels.Where(u => 
                    (!string.IsNullOrEmpty(u.FullName) && u.FullName.ToLower().Contains(searchString)) ||
                    (!string.IsNullOrEmpty(u.Email) && u.Email.ToLower().Contains(searchString))
                );
            }

            return View(viewModels);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userService.GetUserProfileAsync(id);
            if (user == null) return NotFound();

            var viewModel = _mapper.Map<UserViewModel>(user);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBan(string id)
        {
            var result = await _userService.ToggleBanUserAsync(id);
            if (result) TempData["Success"] = "Đã cập nhật trạng thái hoạt động của tài khoản.";
            else TempData["Error"] = "Cập nhật trạng thái thất bại. Vui lòng thử lại.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ReportedUsers()
        {
            var usersDto = await _userService.GetReportedUsersAsync();
            var viewModels = _mapper.Map<IEnumerable<UserViewModel>>(usersDto);
            return View(viewModels);
        }

        // Đã chuẩn tham số ID để load chi tiết hồ sơ
        public async Task<IActionResult> ViewReports(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("PendingReports");

            // Lấy lịch sử vi phạm
            var reportsDto = await _reportService.GetReportsForUserAsync(id);
            var viewModels = _mapper.Map<IEnumerable<ReportViewModel>>(reportsDto);

            // Lấy thông tin User để hiện tên
            var user = await _userService.GetUserProfileAsync(id);
            
            // Đặt tên ViewBag khớp với View
            ViewBag.TargetUser = user?.FullName ?? "Người dùng";
            ViewBag.TargetUserId = id; 
            
            return View(viewModels);
        }

        // Hỗ trợ giao diện 2 Tab (Dashboard)
        public async Task<IActionResult> PendingReports()
        {
            var pendingDto = await _reportService.GetAllPendingReportsAsync();
            
            var resolvedDto = await _reportService.GetResolvedReportsAsync(); 

            var viewModel = new ReportDashboardViewModel
            {
                PendingReports = _mapper.Map<IEnumerable<ReportViewModel>>(pendingDto),
                ResolvedReports = _mapper.Map<IEnumerable<ReportViewModel>>(resolvedDto)
            };
            return View(viewModel);
        }

        // Dùng hàm PenalizeUserAsync (Trừ 10 điểm, +1 gậy)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Penalize(string userId, int reportId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Không tìm thấy ID người dùng để xử phạt.";
                return RedirectToAction("PendingReports");
            }

            // Gọi hàm trừ 10 điểm, +1 gậy
            var success = await _userService.PenalizeUserAsync(userId, 10, 1);
            
            if (success)
            {
                await _reportService.ResolveWithActionAsync(reportId, "Admin đã xử phạt (Trừ 10 điểm, 1 gậy cảnh cáo).");
                TempData["Success"] = "Đã xử phạt tài khoản thành công (-10 điểm, +1 gậy).";
            }
            else
            {
                TempData["Error"] = "Tài khoản không tồn tại hoặc đã bị khóa trước đó.";
            }
            
            return RedirectToAction("PendingReports");
        }

        // Bỏ qua báo cáo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DismissReport(int reportId)
        {
            await _reportService.ResolveWithActionAsync(reportId, "Admin đã bỏ qua báo cáo này.");
            TempData["Success"] = "Đã bỏ qua báo cáo.";
            return RedirectToAction("PendingReports");
        }
    }
}