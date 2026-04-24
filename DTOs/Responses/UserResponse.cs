namespace StudyShare.DTOs.Responses
{
    public class UserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public int Points { get; set; }
        public int WarningCount { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool IsBanned { get; set; }
        // public bool IsBanned => LockoutEnd.HasValue && LockoutEnd.Value > DateTimeOffset.Now;
    }
}