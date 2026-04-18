using StudyShare.DTOs.Responses;
namespace StudyShare.Services.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<ReportResponse>> GetAllReportsAsync();
        Task<bool> DeleteReportAsync(int reportId);
    }
}
