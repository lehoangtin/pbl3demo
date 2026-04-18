using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;

namespace StudyShare.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<IEnumerable<QuestionResponse>> GetAllAsync();
        Task<QuestionResponse?> GetByIdAsync(int id);
        Task<QuestionUpdateRequest?> GetForEditAsync(int id);
        // Chú ý: Hàm Create cần nhận thêm userId để gán cho câu hỏi
        Task<bool> CreateAsync(QuestionCreateRequest request, string userId);
        Task<bool> UpdateAsync(QuestionUpdateRequest request, string currentUserId, bool isAdmin);
        Task<bool> DeleteAsync(int id, string currentUserId, bool isAdmin);
    }
}