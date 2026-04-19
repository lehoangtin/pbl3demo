namespace StudyShare.ViewModels
{
    public class QuestionViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatar { get; set; }
        public int AnswerCount { get; set; }
        
        // Danh sách câu trả lời cũng nên chuyển sang ViewModel
        public List<AnswerViewModel> Answers { get; set; } = new();
    }

    public class AnswerViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatar { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}