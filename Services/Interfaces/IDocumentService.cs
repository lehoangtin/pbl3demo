using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
using StudyShare.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyShare.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<IEnumerable<DocumentResponse>> GetAllAsync();
        Task<DocumentResponse?> GetByIdAsync(int id);
        Task<DocumentUpdateRequest?> GetForEditAsync(int id);
        Task<bool> UpdateAsync(DocumentUpdateRequest request, string currentUserId, bool isAdmin);
        Task<bool> CreateAsync(DocumentCreateRequest request, string userId);
        Task<bool> DeleteAsync(int id, string currentUserId, bool isAdmin);
        Task<bool> DeleteByUserAsync(int id, string userId);
        Task<IEnumerable<Document>> GetUserDocumentsAsync(string userId);
        Task<IEnumerable<DocumentResponse>> GetAllForAdminAsync(string search);
        Task<DocumentResponse?> GetDetailsForReviewAsync(int id);
        Task<bool> ApproveDocumentAsync(int id);
        // Thêm 2 hàm này dùng cho trang chủ
        Task<IEnumerable<DocumentResponse>> GetApprovedDocumentsAsync(string searchTerm, int? categoryId);
        Task<IEnumerable<DocumentResponse>> GetAllApprovedAsync();
        Task<DocumentResponse?> GetDocumentDetailsAsync(int id);
        Task<bool>DeleteByAdminAsync(int id);
    }
}