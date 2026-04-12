using Microsoft.AspNetCore.Identity;
using System;

namespace StudyShare.Models
{
    public class AppUser : IdentityUser
    {
        // 🔥 thêm field của riêng bạn

        public string FullName { get; set; }

        public string? Avatar { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int Points { get; set; } = 0; 
        public int WarningCount { get; set; } = 0;
public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
        public bool IsBanned { get; set; } = false;
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();      
    public virtual ICollection<SavedDocument> SavedDocuments { get; set; } = new List<SavedDocument>();
    }
}