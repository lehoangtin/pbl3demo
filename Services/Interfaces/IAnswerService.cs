using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;  
using StudyShare.Services.Interfaces;
using StudyShare.Repositories.Interfaces; 
namespace StudyShare.Services.Interfaces
{
    public interface IAnswerService
    {
        Task<bool> CreateAsync(AnswerCreateRequest request, string userId);
        Task<bool> DeleteAsync(int id, string currentUserId, bool isAdmin);
        Task<bool> DeleteByUserAsync(int id, string userId);
        Task<bool> DeleteByAdminAsync(int id);
        Task<IEnumerable<AnswerResponse>> GetByQuestionIdAsync(int questionId);

    }
}
