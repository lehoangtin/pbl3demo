using System.ComponentModel.DataAnnotations;

// Dòng này rất quan trọng để View có thể nhận diện được Model
namespace StudyShare.ViewModels 
{
public class ResendEmailConfirmationViewModel
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty; // Khởi tạo giá trị mặc định để hết cảnh báo CS8618
}
}