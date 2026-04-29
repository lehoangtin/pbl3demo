using System;
namespace StudyShare.DTOs.Responses
{
    public class QuestionResponse
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        // Những dữ liệu lấy từ bảng khác sang
        public string AuthorName { get; set; } = string.Empty; 
        public string AuthorEmail { get; set; } = string.Empty; // THÊM DÒNG NÀY
        public int AnswerCount { get; set; } 
        public string UserId { get; set; } = string.Empty;
        public IEnumerable<AnswerResponse> Answers { get; set; } = new List<AnswerResponse>();
        
    }
}