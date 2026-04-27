namespace StudyShare.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public int Points { get; set; }
        public bool IsBanned { get; set; }
        public int WarningCount { get; set; }
        public int DocumentCount { get; set; }
        public int QuestionCount { get; set; }
    }
}