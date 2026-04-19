namespace StudyShare.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int BannedUsers { get; set; }
        public int TotalDocuments { get; set; }
        public int ApprovedDocuments { get; set; }
        public int PendingDocuments { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalAnswers { get; set; }
        public int TotalCategories { get; set; }

        // Danh sách hiển thị trên Dashboard
        public List<UserViewModel> TopUsers { get; set; } = new();
        public List<DocumentViewModel> RecentPendingDocs { get; set; } = new();
    }
}
