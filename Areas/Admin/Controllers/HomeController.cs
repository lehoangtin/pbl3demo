using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.Services.Interfaces;
using StudyShare.ViewModels;
using System.Threading.Tasks;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly IDashboardService   _dashboardService;

        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            // Giả sử hàm GetStats của bạn trả về một object chứa số lượng User, Doc, Question
            var stats = await _dashboardService.GetAdminDashboardStatsAsync();
            
            // Bạn nên tạo AdminDashboardViewModel để chứa các con số này
            var viewModel = new AdminDashboardViewModel 
            {
                TotalUsers = stats.TotalUsers,
                TotalDocuments = stats.TotalDocuments,
                TotalQuestions = stats.TotalQuestions,
                PendingDocuments = stats.PendingDocuments
            };

            return View(viewModel);
        }
    }
}
