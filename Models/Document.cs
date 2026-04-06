using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyShare.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string FilePath { get; set; }

        public string FileName { get; set; }

        public string FileType { get; set; }

        public long FileSize { get; set; }

        public int DownloadCount { get; set; } = 0;

        public DateTime UploadDate { get; set; } = DateTime.Now;
// 🔥 THÊM CÁC DÒNG NÀY ĐỂ FIX LỖI
        public bool IsApproved { get; set; } = false; 

        public string UserId { get; set; } // ID người đăng
        
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } // Thuộc tính dẫn hướng
    }
}