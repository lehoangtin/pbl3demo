   namespace StudyShare.ViewModels
{
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