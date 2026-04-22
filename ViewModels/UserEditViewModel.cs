using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudyShare.ViewModels
{
    public class UserEditViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;
        [Display(Name = "Email")]
        public string? Email { get; set; }
        [Display(Name = "Ảnh đại diện")]
        public IFormFile? AvatarFile { get; set; }
        public string? Avatar { get; set; }

        public string? CurrentAvatarUrl { get; set; }
        [Display(Name = "Số điểm")]
        public int Points { get; set; }

        [Display(Name = "Số gậy cảnh cáo")]
        public int WarningCount { get; set; }

        [Display(Name = "Trạng thái khóa")]
        public bool IsBanned { get; set; }
    }
}