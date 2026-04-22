using System.ComponentModel.DataAnnotations;

namespace StudyShare.ViewModels
{
    public class QuestionEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống")]
        [Display(Name = "Nội dung câu hỏi")]
        public string Content { get; set; } = string.Empty;
    }
}