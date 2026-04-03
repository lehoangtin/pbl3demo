using Microsoft.AspNetCore.Identity;
using System;

namespace StudyShare.Models
{
    public class AppUser : IdentityUser
    {
        // 🔥 thêm field của riêng bạn

        public string FullName { get; set; }

        public string Avatar { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<Question> Questions { get; set; }
        public List<Answer> Answers { get; set; }
    }
}