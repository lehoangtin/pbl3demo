using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;

namespace StudyShare.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
        Task<IEnumerable<UserResponse>> GetTopRankingAsync(int topCount);
        Task<ProfileUpdateRequest?> GetProfileForEditAsync(string userId);
        Task<bool> UpdateProfileAsync(ProfileUpdateRequest request);
        Task<bool> ToggleBanUserAsync(string userId); // Khóa/Mở khóa tài khoản
    }
}