using Microsoft.AspNetCore.Identity;
using StudyShare.Models;
using StudyShare.ViewModels;
using System.Threading.Tasks;

namespace StudyShare.Services.Interfaces
{
    public interface IAuthService
    {
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task<IdentityResult> RegisterAsync(RegisterViewModel model);
        Task LogoutAsync();
        
        Task<AppUser?> GetUserByIdAsync(string userId);
        Task<AppUser?> GetUserByEmailAsync(string email);
        
        // Xác nhận Email
        Task<string> GenerateEmailConfirmationTokenAsync(AppUser user);
        Task<IdentityResult> ConfirmEmailAsync(AppUser user, string code);
        Task<bool> IsEmailConfirmedAsync(AppUser user);
        
        // Quên mật khẩu
        Task<string> GeneratePasswordResetTokenAsync(AppUser user);
        Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string newPassword);
    }
}