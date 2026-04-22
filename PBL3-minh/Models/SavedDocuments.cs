using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyShare.Models // 🔥 Phải khớp với namespace trong file AppUser.cs và Document.cs
{
    public class SavedDocument
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } // Lỗi dòng 8 fix ở đây

        public int DocumentId { get; set; }
        
        [ForeignKey("DocumentId")]
        public virtual Document Document { get; set; } // Lỗi dòng 9 fix ở đây

        public DateTime SavedDate { get; set; } = DateTime.Now;
    }
}