using StudyShare.Models;

namespace StudyShare.Repositories.Interfaces
{
    public interface IAnswerRepository
    {
        Task<Answer?> GetByIdAsync(int id);
        Task<Answer?> GetByIdAndUserAsync(int id, string userId);
        Task<bool> CreateAsync(Answer answer);
        Task<bool> DeleteAsync(Answer answer);
        Task<bool> DeleteByUserAsync(Answer answer);
        Task<IEnumerable<Answer>> GetAllAsync();

    }
}