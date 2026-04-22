using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyShare.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        // --- Foreign Keys ---
        public string? ReporterUserId { get; set; }
        public string TargetUserId { get; set; }
        
        public int? DocumentId { get; set; } // 🔥 Bổ sung trường này
        public int? QuestionId { get; set; }
        public int? AnswerId { get; set; }
        
        [Required]
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- Navigation Properties ---
        
        [ForeignKey("ReporterUserId")]
        public virtual AppUser Reporter { get; set; }

        [ForeignKey("TargetUserId")]
        public virtual AppUser Target { get; set; }

        [ForeignKey("DocumentId")]
        public virtual Document Document { get; set; } // 🔥 Thêm dòng này để hết lỗi CS1061

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } 

        [ForeignKey("AnswerId")]
        public virtual Answer Answer { get; set; }   
        public bool IsResolved { get; set; } = true; // Đánh dấu đã xử lý chưa
        public string? ActionTaken { get; set; } // Ghi chú kết quả: Đã khóa, Đã cảnh cáo, Bỏ qua...  
    }
}