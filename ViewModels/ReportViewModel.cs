namespace StudyShare.ViewModels
{
    public class ReportViewModel
    {
        public int Id { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        // Thông tin người gửi báo cáo
        public string ReporterId { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;

        // Phân loại đối tượng bị báo cáo (Tài liệu, Câu hỏi, Câu trả lời, Người dùng)
        public string TargetType { get; set; } = string.Empty; 
        
        // Thông tin chi tiết của đối tượng bị báo cáo (để hiển thị tiêu đề hoặc tên)
        public string TargetName { get; set; } = string.Empty; 
        
        // Các ID liên kết (để Admin có thể bấm vào xem chi tiết)
        public int? DocumentId { get; set; }
        public int? QuestionId { get; set; }
        public int? AnswerId { get; set; }
        public string? TargetUserId { get; set; }

        // Trạng thái xử lý
        public bool IsResolved { get; set; }
        public string? ActionTaken { get; set; }
    }
}