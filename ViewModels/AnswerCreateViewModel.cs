using System.ComponentModel.DataAnnotations;

namespace StudyShare.ViewModels
{
    public class AnswerCreateViewModel
    {
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Nội dung câu trả lời không được để trống")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;
    }
}