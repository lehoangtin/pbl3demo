using StudyShare.Models;

namespace StudyShare.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<AppUser>> GetAllUsersAsync();
        Task<IEnumerable<AppUser>> GetTopRankingAsync(int topCount);
        Task<AppUser?> GetByIdAsync(string userId);
        Task<AppUser?> GetUserProfileWithIncludesAsync(string userId);
        Task<bool> UpdateUserAsync(AppUser user);
        Task<DateTimeOffset?> GetLockoutEndDateAsync(AppUser user);
        Task SetLockoutEndDateAsync(AppUser user, DateTimeOffset? lockoutEnd);
        Task<bool> AddSavedDocumentAsync(SavedDocument document);
        Task<bool> RemoveSavedDocumentAsync(SavedDocument document);
        Task<SavedDocument?> GetSavedDocumentAsync(string userId, int documentId);
        Task<bool> IsDocumentSavedAsync(string userId, int documentId);
        Task<IEnumerable<SavedDocument>> GetSavedDocumentsListAsync(string userId);
        Task<IEnumerable<AppUser>> GetAllAsync();
        Task <IEnumerable<AppUser>> GetReportedUsersAsync();   
        
    }
}