using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudyShare.Models;

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
        Task<bool> DeleteByUserAsync(int id, string userId);
        Task<IEnumerable<Question>> GetUserQuestionsAsync(string userId);
        Task<IEnumerable<Question>> GetAllForAdminAsync();
        Task<Question?> GetDetailsForAdminAsync(int id);
        Task<IEnumerable<Report>> GetReportsForQuestionAsync(int questionId);
        Task<bool> DeleteQuestionByAdminAsync(int id);
        Task<bool> DeleteAnswerByAdminAsync(int id);
    }
}