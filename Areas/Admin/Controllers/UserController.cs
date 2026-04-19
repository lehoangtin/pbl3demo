using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;
using StudyShare.ViewModels; // Đảm bảo đã thêm namespace này
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
            // Lấy danh sách DTO từ Service
            var usersDto = await _userService.GetAllUsersAsync();
            
            // SỬA: Map sang danh sách ViewModel
            var viewModels = _mapper.Map<IEnumerable<UserViewModel>>(usersDto);
            
            return View(viewModels);
        }

        public async Task<IActionResult> Details(string id)
        {
            var userDto = await _userService.GetUserProfileAsync(id);
            if (userDto == null) return NotFound();

            // SỬA: Map sang ViewModel
            var viewModel = _mapper.Map<UserViewModel>(userDto);
            
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userService.GetUserProfileAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserResponse model)
        {
            if (ModelState.IsValid)
            {
                await _userService.UpdateUserByAdminAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBan(string id)
        {
            await _userService.ToggleBanUserAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ReportedUsers()
        {
            // Lấy danh sách DTO người dùng bị báo cáo
            var usersDto = await _userService.GetReportedUsersAsync();
            
            // SỬA: Map sang danh sách ViewModel
            var viewModels = _mapper.Map<IEnumerable<UserViewModel>>(usersDto);
            
            return View(viewModels);
        }

        public async Task<IActionResult> ViewReports(string userId)
        {
            // Lấy danh sách DTO báo cáo
            var reportsDto = await _reportService.GetReportsForUserAsync(userId);
            
            // SỬA: Map sang danh sách ViewModel (Dùng ReportViewModel bạn vừa tạo)
            var viewModels = _mapper.Map<IEnumerable<ReportViewModel>>(reportsDto);
            
            ViewBag.TargetUserId = userId;
            return View(viewModels);
        }
    }
}
