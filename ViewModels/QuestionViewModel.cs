namespace StudyShare.ViewModels
{
    public class QuestionViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatar { get; set; }
        public int AnswerCount { get; set; }
        public string AuthorEmail { get; set; } = string.Empty; // THÊM DÒNG NÀY
        
        // Danh sách câu trả lời cũng nên chuyển sang ViewModel
        public IEnumerable<AnswerViewModel> Answers { get; set; } = new List<AnswerViewModel>();
    }
}