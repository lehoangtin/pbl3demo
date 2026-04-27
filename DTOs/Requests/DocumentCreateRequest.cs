using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudyShare.DTOs.Requests
{
    public class DocumentCreateRequest
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [Display(Name = "Tiêu đề tài liệu")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Mô tả ngắn")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn file để tải lên")]
        [Display(Name = "File tài liệu")]
        public IFormFile File { get; set; } = null!;
    }
}