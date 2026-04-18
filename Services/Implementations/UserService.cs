using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;

namespace StudyShare.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public UserService(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return _mapper.Map<IEnumerable<UserResponse>>(users);
        }

        public async Task<IEnumerable<UserResponse>> GetTopRankingAsync(int topCount)
        {
            var topUsers = await _userManager.Users
                .OrderByDescending(u => u.Points)
                .Take(topCount)
                .ToListAsync();
            return _mapper.Map<IEnumerable<UserResponse>>(topUsers);
        }

        public async Task<ProfileUpdateRequest?> GetProfileForEditAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;
            
            return new ProfileUpdateRequest { Id = user.Id, FullName = user.FullName };
        }

        public async Task<bool> UpdateProfileAsync(ProfileUpdateRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null) return false;

            user.FullName = request.FullName;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ToggleBanUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Nếu đang bị khóa -> Mở khóa. Nếu đang mở -> Khóa 100 năm.
            var lockoutEndDate = await _userManager.GetLockoutEndDateAsync(user);
            if (lockoutEndDate.HasValue && lockoutEndDate > DateTimeOffset.Now)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            return true;
        }
    }
}