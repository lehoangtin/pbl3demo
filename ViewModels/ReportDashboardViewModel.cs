using System.Collections.Generic;

namespace StudyShare.ViewModels
{
    public class ReportDashboardViewModel
    {
        // Chứa danh sách các báo cáo đang đợi Admin
        public IEnumerable<ReportViewModel> PendingReports { get; set; }
        // Chứa danh sách lịch sử vi phạm/AI chặn
        public IEnumerable<ReportViewModel> ResolvedReports { get; set; }
    }
}