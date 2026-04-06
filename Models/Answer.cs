using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace StudyShare.Models
{
    public class Answer
{
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập câu trả lời")]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }

        public string UserId { get; set; } // ID người trả lời
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }
}
}
