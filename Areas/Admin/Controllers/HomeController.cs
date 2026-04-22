using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.Services.Interfaces;
using StudyShare.ViewModels;
using System.Threading.Tasks;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Implementations;
namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly IDashboardService   _dashboardService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IDocumentService _documentService;

        public HomeController(IDashboardService dashboardService, IMapper mapper, IUserService userService, IDocumentService documentService)
        {
            _dashboardService = dashboardService;
            _mapper = mapper;
            _userService = userService;
            _documentService = documentService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _dashboardService.GetAdminDashboardStatsAsync();
    
            // Giả sử service của bạn cũng cung cấp TopUsers và PendingDocs
            // Nếu chưa có, bạn hãy gọi thêm hàm từ UserService/DocumentService để lấy
            var topUsersDto = await _userService.GetTopRankingAsync(5); // Giả sử lấy top 5
            var pendingDocsDto = await _documentService.GetPendingDocumentsAsync();
            var viewModel = new AdminDashboardViewModel 
            {
                TotalUsers = stats.TotalUsers,
                BannedUsers = stats.BannedUsers,
                TotalDocuments = stats.TotalDocuments,
                ApprovedDocuments = stats.ApprovedDocuments,
                PendingDocuments = stats.PendingDocuments,
                TotalQuestions = stats.TotalQuestions,
                TotalAnswers = stats.TotalAnswers,
                TotalCategories = stats.TotalCategories,
                
                // Gán dữ liệu danh sách vào ViewModel
            TopUsers = _mapper.Map<List<UserViewModel>>(topUsersDto),
            RecentPendingDocs = _mapper.Map<List<DocumentViewModel>>(pendingDocsDto)            };

            return View(viewModel);
        }
    }
}
