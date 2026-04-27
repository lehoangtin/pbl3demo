using StudyShare.Models;

namespace StudyShare.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<IEnumerable<Report>> GetReportsByTargetUserAsync(string userId);
        Task UpdateAsync(Report report);
        Task<Report?> GetByIdAsync(int id);
        Task<IEnumerable<Report>> GetAllResolvedReportsAsync();
        Task<IEnumerable<Report>> GetAllPendingReportsAsync(); // Lấy tất cả báo cáo chưa được xử lý
        Task<Report> CreateAsync(Report report);
    }
}
