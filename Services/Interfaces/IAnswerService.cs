using StudyShare.DTOs.Requests;
namespace StudyShare.Services.Interfaces
{
    public interface IAnswerService
    {
        Task<bool> CreateAsync(AnswerCreateRequest request, string userId);
        Task<bool> DeleteAsync(int id, string currentUserId, bool isAdmin);
        Task<bool> DeleteByUserAsync(int id, string userId);
    }
}
