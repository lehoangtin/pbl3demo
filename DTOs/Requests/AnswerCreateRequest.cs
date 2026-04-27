using System.ComponentModel.DataAnnotations;
namespace StudyShare.DTOs.Requests
{
    public class AnswerCreateRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; } = string.Empty;
        public int QuestionId { get; set; }
    }
}