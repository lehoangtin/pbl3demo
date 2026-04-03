using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StudyShare.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔥 thêm user
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public List<Answer> Answers { get; set; } = new List<Answer>();
        
    }
}