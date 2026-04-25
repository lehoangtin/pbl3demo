using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;
using StudyShare.ViewModels;
using System.Linq; // BẮT BUỘC THÊM DÒNG NÀY
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
            ViewData["CurrentFilter"] = searchString; // Trả về view để giữ lại từ khóa trên ô tìm kiếm

            var users = await _userService.GetAllUsersAsync();
            var viewModels = _mapper.Map<IEnumerable<UserViewModel>>(users);

            // Xử lý tìm kiếm
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
            return View(viewModel);;
        }

[HttpPost]
public async Task<IActionResult> ToggleBan(string id)
{
    var result = await _userService.ToggleBanUserAsync(id);
    
    // Đã kiểm tra result
    if (result)
    {
        TempData["Success"] = "Đã cập nhật trạng thái hoạt động của tài khoản.";
    }
    else
    {
        TempData["Error"] = "Cập nhật trạng thái thất bại. Vui lòng thử lại.";
    }
    
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

// 2. Action Xử phạt: Tăng cảnh cáo và tự động khóa khi đủ 3 lần
// Action Xử phạt: Khóa luôn tài khoản khi có vi phạm
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Penalize(string userId, int reportId)
{
    // 1. Gọi thẳng hàm ToggleBanUserAsync (hàm khóa tài khoản mà chúng ta đã sửa lỗi ở trên)
    // Nếu họ đang hoạt động -> Gọi hàm này sẽ khóa họ lại (IsBanned = true)
    var success = await _userService.ToggleBanUserAsync(userId);
    
    if (success)
    {
        // 2. Cập nhật trạng thái báo cáo là đã giải quyết
        await _reportService.ResolveWithActionAsync(reportId, "Admin đã xử lý vi phạm và KHÓA tài khoản này.");
        TempData["Success"] = "Đã khóa tài khoản vi phạm thành công.";
    }
    else
    {
        TempData["Error"] = "Có lỗi xảy ra khi khóa tài khoản.";
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