using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyShare.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }
        public string ReporterUserId { get; set; }
        public string TargetUserId { get; set; }
        
        public int? QuestionId { get; set; }
        public int? AnswerId { get; set; }
        
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- Navigation Properties (BẮT BUỘC PHẢI CÓ ĐỂ HẾT LỖI) ---
        
        [ForeignKey("ReporterUserId")]
        public virtual AppUser Reporter { get; set; }

        [ForeignKey("TargetUserId")]
        public virtual AppUser Target { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } // Lỗi do thiếu dòng này

        [ForeignKey("AnswerId")]
        public virtual Answer Answer { get; set; }     // Lỗi do thiếu dòng này
    }
}