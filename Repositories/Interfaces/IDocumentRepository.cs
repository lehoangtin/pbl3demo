using StudyShare.Models;
using StudyShare.DTOs.Requests;

namespace StudyShare.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
        Task<IEnumerable<Document>> GetAllAsync();
        Task<Document?> GetByIdAsync(int id);
        Task<Document?> GetForEditAsync(int id);
        Task<bool> UpdateAsync(Document document);
        Task<bool> CreateAsync(Document document);
        Task<bool> DeleteAsync(Document document);
        Task<IEnumerable<Document>> GetAllForAdminAsync(string search);
        Task<Document?> GetDetailsForReviewAsync(int id);
        Task<bool> ApproveDocumentAsync(Document document);
        Task<IEnumerable<Document>> GetUserDocumentsAsync(string userId);
        Task<bool> DeleteByUserAsync(Document document);
        Task<IEnumerable<Document>> GetApprovedDocumentsAsync(string searchTerm, int? categoryId);
        Task<Document?> GetDocumentDetailsAsync(int id);
        Task<IEnumerable<Document>> GetPendingDocumentsAsync();

    }
}