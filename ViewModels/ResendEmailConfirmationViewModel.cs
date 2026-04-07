using System.ComponentModel.DataAnnotations;

// Dòng này rất quan trọng để View có thể nhận diện được Model
namespace StudyShare.ViewModels 
{
    public class ResendEmailConfirmationViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
    }
}