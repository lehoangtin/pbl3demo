using System.ComponentModel.DataAnnotations;

namespace StudyShare.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Token { get; set; } = default!; // Đổi từ Code thành Token
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải dài từ {2} đến {1} ký tự.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = default!;
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = default!;
    }
}