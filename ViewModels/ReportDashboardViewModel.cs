using System.Collections.Generic;

namespace StudyShare.ViewModels
{
    public class ReportDashboardViewModel
    {
        public IEnumerable<ReportViewModel> PendingReports { get; set; }
        public IEnumerable<ReportViewModel> ResolvedReports { get; set; }
    }
}