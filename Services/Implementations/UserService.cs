using AutoMapper;
using Microsoft.AspNetCore.Http;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;
using StudyShare.Repositories.Interfaces; // Add Repository

namespace StudyShare.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository; // Inject Repository thay vì DBContext
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IWebHostEnvironment env, IMapper mapper)
        {
            _userRepository = userRepository;
            _env = env;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return _mapper.Map<IEnumerable<UserResponse>>(users);
        }

        public async Task<IEnumerable<UserResponse>> GetTopRankingAsync(int topCount)
        {
            var topUsers = await _userRepository.GetTopRankingAsync(topCount);
            return _mapper.Map<IEnumerable<UserResponse>>(topUsers);
        }

        public async Task<ProfileUpdateRequest?> GetProfileForEditAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;
            
            return new ProfileUpdateRequest { Id = user.Id, FullName = user.FullName };
        }

        public async Task<bool> UpdateProfileAsync(ProfileUpdateRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null) return false;

            user.FullName = request.FullName;
            return await _userRepository.UpdateUserAsync(user);
        }

        public async Task<bool> ToggleBanUserAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            var lockoutEndDate = await _userRepository.GetLockoutEndDateAsync(user);
            if (lockoutEndDate.HasValue && lockoutEndDate > DateTimeOffset.Now)
            {
                await _userRepository.SetLockoutEndDateAsync(user, null); // Mở khóa
            }
            else
            {
                await _userRepository.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue); // Khóa
            }
            return true;
        }

        public async Task<AppUser?> GetUserProfileAsync(string userId)
        {
            return await _userRepository.GetUserProfileWithIncludesAsync(userId);
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, AppUser model, IFormFile? avatarFile)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.FullName = model.FullName;
            user.Email = model.Email;

            // Nghiệp vụ xử lý file nằm ở lớp Business Logic
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var ext = Path.GetExtension(avatarFile.FileName);
                var fileName = Guid.NewGuid() + ext;
                var path = Path.Combine(_env.WebRootPath, "images", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await avatarFile.CopyToAsync(stream);
                user.Avatar = "/images/" + fileName;
            }
            
            return await _userRepository.UpdateUserAsync(user);
        }

        public async Task<bool> SaveDocumentAsync(string userId, int docId)
        {
            if (await _userRepository.IsDocumentSavedAsync(userId, docId)) return false;
            
            var savedDoc = new SavedDocument { UserId = userId, DocumentId = docId, SavedDate = DateTime.Now };
            return await _userRepository.AddSavedDocumentAsync(savedDoc);
        }

        public async Task<bool> UnsaveDocumentAsync(string userId, int docId)
        {
            var savedDoc = await _userRepository.GetSavedDocumentAsync(userId, docId);
            if (savedDoc == null) return false;
            
            return await _userRepository.RemoveSavedDocumentAsync(savedDoc);
        }

        public async Task<IEnumerable<SavedDocument>> GetSavedDocumentsAsync(string userId)
        {
            return await _userRepository.GetSavedDocumentsListAsync(userId);
        }

        public async Task<bool> IsDocumentSavedAsync(string userId, int documentId)
        {
            return await _userRepository.IsDocumentSavedAsync(userId, documentId);
        }

        public async Task<bool> IsUserBannedAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.IsBanned ?? false;
        }
    }
}