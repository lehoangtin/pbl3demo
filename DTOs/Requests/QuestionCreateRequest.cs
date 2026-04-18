using System.ComponentModel.DataAnnotations;

namespace StudyShare.DTOs.Requests
{
    public class QuestionCreateRequest
    {
        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống")]
        [Display(Name = "Nội dung câu hỏi")]
        public string Content { get; set; } = string.Empty;
    }
}