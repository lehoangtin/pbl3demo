using System.ComponentModel.DataAnnotations;

namespace StudyShare.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        // Mối quan hệ: Một danh mục có nhiều tài liệu
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}