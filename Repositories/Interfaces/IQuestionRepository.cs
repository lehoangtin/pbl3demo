using StudyShare.Models;

namespace StudyShare.Repositories.Interfaces
{
    public interface IQuestionRepository
    {
        Task<IEnumerable<Question>> GetAllAsync();
        Task<Question?> GetByIdAsync(int id);
        Task<Question?> GetForEditAsync(int id);
        Task<bool> CreateAsync(Question question);
        Task<bool> UpdateAsync(Question question);
        Task<bool> DeleteAsync(Question question);
        Task<IEnumerable<Question>> GetAllForAdminAsync();
        Task<Question?> GetDetailsForAdminAsync(int id);
        Task<IEnumerable<Report>> GetReportsForQuestionAsync(int questionId);
        Task<IEnumerable<Question>> GetUserQuestionsAsync(string userId);
    }
}