using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudyShare.ViewModels
{
    public class DocumentEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [Display(Name = "Tiêu đề tài liệu")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Mô tả ngắn")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "File tài liệu")]
        public IFormFile? File { get; set; }
    }
}