using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StudyShare.Models;
using StudyShare.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace StudyShare.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardService(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<dynamic> GetAdminDashboardStatsAsync()
        {
            var normalUsers = await _userManager.GetUsersInRoleAsync("User");

            return new
            {
                TotalUsers = await _context.Users.CountAsync(),
                BannedUsers = await _context.Users.CountAsync(u => u.IsBanned),
                TotalDocuments = await _context.Documents.CountAsync(),
                ApprovedDocuments = await _context.Documents.CountAsync(d => d.IsApproved),
                PendingDocuments = await _context.Documents.CountAsync(d => !d.IsApproved),
                TotalQuestions = await _context.Questions.CountAsync(),
                TotalAnswers = await _context.Answers.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TopUsers = normalUsers.OrderByDescending(u => u.Points).Take(5).ToList(),
                RecentPendingDocs = await _context.Documents
                    .Include(d => d.User)
                    .Where(d => !d.IsApproved)
                    .OrderByDescending(d => d.UploadDate)
                    .Take(5)
                    .ToListAsync()
            };
        }
    }
}