using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using StudyShare.Services.Interfaces;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _dashboardService.GetAdminDashboardStatsAsync();
            
            ViewBag.TotalUsers = stats.TotalUsers;
            ViewBag.BannedUsers = stats.BannedUsers;
            ViewBag.TotalDocuments = stats.TotalDocuments;
            ViewBag.ApprovedDocuments = stats.ApprovedDocuments;
            ViewBag.PendingDocuments = stats.PendingDocuments;
            ViewBag.TotalQuestions = stats.TotalQuestions;
            ViewBag.TotalAnswers = stats.TotalAnswers;
            ViewBag.TotalCategories = stats.TotalCategories;
            ViewBag.TopUsers = stats.TopUsers;
            ViewBag.RecentPendingDocs = stats.RecentPendingDocs;

            return View();
        }
    }
}