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
        Task<AppUser?> GetUserByEmailAsync(string email);
        Task<string> GeneratePasswordResetTokenAsync(AppUser user);
        Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string newPassword);
        Task<string> GenerateEmailConfirmationTokenAsync(AppUser user);
    }
}