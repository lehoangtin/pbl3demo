using System.ComponentModel.DataAnnotations;

namespace StudyShare.ViewModels
{
    public class QuestionCreateViewModel
    {
        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống")]
        [Display(Name = "Nội dung câu hỏi")]
        public string Content { get; set; } = string.Empty;
    }
}