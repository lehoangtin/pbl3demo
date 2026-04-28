using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
using StudyShare.Models;
using Microsoft.AspNetCore.Http;

namespace StudyShare.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
        Task<IEnumerable<UserResponse>> GetTopRankingAsync(int topCount);
        Task<ProfileUpdateRequest?> GetProfileForEditAsync(string userId);
        Task<bool> UpdateProfileAsync(ProfileUpdateRequest request);
        Task<bool> ToggleBanUserAsync(string userId);
        Task<AppUser?> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, AppUser model, IFormFile? avatarFile);
        Task<bool> SaveDocumentAsync(string userId, int docId);
        Task<bool> UnsaveDocumentAsync(string userId, int docId);
        Task<IEnumerable<SavedDocument>> GetSavedDocumentsAsync(string userId);
        // Thêm 2 hàm này kiểm tra trạng thái User
        Task<bool> IsDocumentSavedAsync(string userId, int documentId);
        Task<bool> IsUserBannedAsync(string userId);
        Task<bool> UpdateUserByAdminAsync(UserResponse model);
        Task<IEnumerable<UserResponse>> GetReportedUsersAsync();
        // Thêm vào file Services/Interfaces/IUserService.cs
        Task<bool> PenalizeUserAsync(string userId, int pointsToDeduct, int warningIncrement);
        Task<bool> AddPointsAsync(string userId, int points);
        Task<bool> UpdateUserRoleAsync(string userId, string targetRole, string currentUserId);
    }
}