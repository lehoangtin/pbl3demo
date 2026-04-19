using StudyShare.Models;

namespace StudyShare.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<IEnumerable<Report>> GetReportsByTargetUserAsync(string userId);
        Task UpdateAsync(Report report);
        Task<Report?> GetByIdAsync(int id);
    }
}
