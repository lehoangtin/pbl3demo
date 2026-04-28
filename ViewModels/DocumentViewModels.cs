namespace StudyShare.ViewModels
{
    public class DocumentViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public int Downloads { get; set; }
        public bool IsApproved { get; set; }
        
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        
        public string UserId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty; // THÊM DÒNG NÀY
        public int Views { get; set; }
        public int DownloadCount { get; set; }
    }
}