using StudyShare.Models;

namespace StudyShare.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<IEnumerable<Report>> GetAllReportsAsync();
        Task<Report?> GetByIdAsync(int id);
        Task<bool> DeleteAsync(Report report);
    }
}