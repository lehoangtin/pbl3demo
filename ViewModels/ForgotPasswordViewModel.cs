using System.ComponentModel.DataAnnotations;

namespace StudyShare.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}