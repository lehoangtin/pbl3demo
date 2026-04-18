using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
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
    }
}