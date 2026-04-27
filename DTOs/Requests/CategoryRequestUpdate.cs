using System.ComponentModel.DataAnnotations;

namespace StudyShare.DTOs.Requests
{
    public class CategoryUpdateRequest
    {
        public int Id { get; set; } // Bắt buộc phải có Id để biết đang sửa dòng nào

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [Display(Name = "Tên danh mục")]
        public string? Name { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }
    }
}