using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StudyShare.Models
{
    public class Answer
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    
    // 🔥 Thêm dòng này để sửa lỗi CS1061 tại Details.cshtml
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int QuestionId { get; set; }
    public virtual Question? Question { get; set; }
    public string UserId { get; set; } = string.Empty;
    public virtual AppUser? User { get; set; }
}
}