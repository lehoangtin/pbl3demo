using StudyShare.DTOs.Responses;
namespace StudyShare.Services.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<ReportResponse>> GetReportsForUserAsync(string userId);
        Task<bool> ResolveReportAsync(int reportId); // Xử lý báo cáo xong

        Task<bool> ResolveWithActionAsync(int reportId, string action); // Xử lý báo cáo xong và ghi lại hành động đã thực hiện
        Task<IEnumerable<ReportResponse>> GetAllPendingReportsAsync(); // Lấy tất cả báo cáo chưa được xử lý
        Task<IEnumerable<ReportResponse>> GetResolvedReportsAsync();
        // Thêm hàm tạo báo cáo tự động
Task<int> CreateAutoReportAsync(string targetUserId, string reason, string actionTaken, int? docId = null, int? qid = null);
    }
}
