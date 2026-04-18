using System.Threading.Tasks;

namespace StudyShare.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<dynamic> GetAdminDashboardStatsAsync();
    }
}