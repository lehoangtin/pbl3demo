namespace StudyShare.Models; // Sửa từ PBL3demo.Models thành StudyShare.Models
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
