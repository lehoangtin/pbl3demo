using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace StudyShare.Models
{
    public class Question
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public string UserId { get; set; } // ID người đặt câu hỏi
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        public virtual List<Answer> Answers { get; set; } = new List<Answer>();
        
    }
}