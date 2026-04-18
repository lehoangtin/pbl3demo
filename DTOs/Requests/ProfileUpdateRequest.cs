using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudyShare.DTOs.Requests
{
    public class ProfileUpdateRequest
    {
        // ID của User (Bắt buộc phải có để hệ thống biết đang cập nhật cho ai)
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 số")]
        public string? PhoneNumber { get; set; }

        // -------------------------------------------------------------------------
        // NẾU DỰ ÁN CỦA BẠN CÓ TÍNH NĂNG UPLOAD ẢNH ĐẠI DIỆN, HÃY MỞ COMMENT CÁC DÒNG DƯỚI ĐÂY:
        // -------------------------------------------------------------------------
        
        // Dùng để nhận file ảnh người dùng upload lên từ form (tương tự như Document)
        [Display(Name = "Ảnh đại diện")]
        public IFormFile? AvatarFile { get; set; }

        // Dùng để hiển thị đường dẫn ảnh cũ (nếu người dùng không chọn upload ảnh mới)
        public string? CurrentAvatarUrl { get; set; }
    }
}