using StudyShare.Models;
using StudyShare.DTOs.Responses;
using System.Collections.Generic;

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
        Task<IEnumerable<Answer>> GetByQuestionIdAsync(int questionId);    
    }
}