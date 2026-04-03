using System.ComponentModel.DataAnnotations;

namespace StudyShare.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}