using System;
using System.ComponentModel.DataAnnotations;

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
        public bool IsApproved { get; set; } = false; // Tài liệu mới tải lên sẽ chờ duyệt
    }
}