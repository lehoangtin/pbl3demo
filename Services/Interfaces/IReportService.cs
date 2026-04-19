using StudyShare.DTOs.Responses;
namespace StudyShare.Services.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<ReportResponse>> GetReportsForUserAsync(string userId);
        Task<bool> ResolveReportAsync(int reportId); // Xử lý báo cáo xong
    }
}
