using System.Threading.Tasks;
using StudyShare.ViewModels;

namespace StudyShare.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardViewModel> GetAdminDashboardStatsAsync();
    }
}