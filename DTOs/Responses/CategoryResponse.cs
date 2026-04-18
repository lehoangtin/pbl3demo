namespace StudyShare.DTOs.Responses
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DocumentCount { get; set; } // Hiển thị số lượng tài liệu
    }
}