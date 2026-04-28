using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;
using StudyShare.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    
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

    // 1. Lấy dữ liệu từ lớp Service (Trả về IEnumerable<UserResponse>)
    var users = await _userService.GetAllUsersAsync();

    // 2. Map sang ViewModel để hiển thị ra View
    var viewModels = _mapper.Map<IEnumerable<UserViewModel>>(users);

    // 3. Thực hiện lọc dữ liệu
    if (!string.IsNullOrEmpty(searchString))
    {
        searchString = searchString.ToLower();
        viewModels = viewModels.Where(u => 
            (!string.IsNullOrEmpty(u.FullName) && u.FullName.ToLower().Contains(searchString)) ||
            (!string.IsNullOrEmpty(u.Email) && u.Email.ToLower().Contains(searchString))
        ).ToList(); // Ép kiểu về List để tránh lỗi truy vấn khi truyền vào View
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
            if (string.IsNullOrEmpty(id)) return NotFound("Không tìm thấy ID người dùng.");

            var reportsDto = await _reportService.GetReportsForUserAsync(id);
            var viewModels = _mapper.Map<IEnumerable<ReportViewModel>>(reportsDto);
            
            ViewBag.TargetUserId = id; 
            
            // Lấy thông tin user để hiển thị tên trên View
            var user = await _userService.GetUserProfileAsync(id);
            ViewBag.TargetUser = user?.FullName ?? "Người dùng ẩn danh";

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
public async Task<IActionResult> Penalize(string userId, int reportId, int pointsDeducted = 10)
{
    if (string.IsNullOrEmpty(userId)) return NotFound("Không tìm thấy ID người dùng.");

    // GỌI ĐÚNG HÀM PHẠT: Sẽ tự động trừ điểm và TĂNG WarningCount lên 1
    // Nếu WarningCount >= 3, hàm này cũng sẽ tự động khóa tài khoản luôn (như bạn đã viết ở UserService)
    var success = await _userService.PenalizeUserAsync(userId, pointsDeducted, 1);
    
    if (success)
    {
        await _reportService.ResolveWithActionAsync(reportId, $"Admin đã xử phạt (Trừ {pointsDeducted} điểm).");
        TempData["Success"] = "Đã xử phạt và ghi nhận 1 lần vi phạm thành công.";
    }
    else
    {
        TempData["Error"] = "Có lỗi xảy ra khi xử phạt.";
    }
    
    return RedirectToAction(nameof(PendingReports));
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
        [HttpPost]
    public async Task<IActionResult> ChangeRole(string userId, string role)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var success = await _userService.UpdateUserRoleAsync(userId, role, currentUserId);

        if (success) TempData["Success"] = "Cập nhật quyền hạn thành công!";
        else TempData["Error"] = "Bạn không có đủ thẩm quyền thực hiện thao tác này!";

        return RedirectToAction("Index");
    }
    [HttpGet]
[Authorize(Roles = "SuperAdmin")] // Chỉ Super Admin mới được vào trang này
public async Task<IActionResult> ManageRoles(string searchString)
{
    var users = await _userService.GetAllUsersAsync();
    var viewModels = _mapper.Map<IEnumerable<UserViewModel>>(users);

    if (!string.IsNullOrEmpty(searchString))
    {
        searchString = searchString.ToLower();
        viewModels = viewModels.Where(u => 
            u.FullName.ToLower().Contains(searchString) || 
            u.Email.ToLower().Contains(searchString)
        ).ToList();
    }

    return View(viewModels);
}
    }
}