using System.ComponentModel.DataAnnotations;

namespace StudyShare.DTOs.Requests
{
    public class CategoryCreateRequest
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }
    }
}