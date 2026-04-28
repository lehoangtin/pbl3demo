using AutoMapper;
using Microsoft.AspNetCore.Http;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;
using StudyShare.Repositories.Interfaces; // Add Repository
using Microsoft.AspNetCore.Identity; // Add Identity for UserManager
using Microsoft.EntityFrameworkCore; // Add EF Core for ToListAsync
using StudyShare.ViewModels;
using StudyShare.Repositories.Implementations; // Add Repository

namespace StudyShare.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository; // Inject Repository thay vì DBContext
        private readonly UserManager<AppUser> _userManager; // Add UserManager for Identity operations
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, UserManager<AppUser> userManager, IWebHostEnvironment env, IMapper mapper)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _env = env;
            _mapper = mapper;
        }

public async Task<IEnumerable<UserResponse>> GetAllUsersAsync() 
{
    var users = await _userManager.Users.ToListAsync();
    
    // Map từ AppUser sang UserResponse
    var userResponses = _mapper.Map<IEnumerable<UserResponse>>(users);

    foreach (var response in userResponses)
    {
        var userEntity = users.First(u => u.Id == response.Id);
        var roles = await _userManager.GetRolesAsync(userEntity);
        
        // Đảm bảo trong class UserResponse của bạn có thuộc tính Role
        response.Role = roles.FirstOrDefault() ?? "User";
    }

    return userResponses;
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

            // 1. Đảo ngược giá trị của biến IsBanned
            user.IsBanned = !user.IsBanned;

            // 🔥 THÊM ĐOẠN NÀY: NẾU ADMIN MỞ KHÓA THÌ RESET SỐ LẦN CẢNH CÁO VỀ 0 ĐỂ CHO HỌ CƠ HỘI LÀM LẠI
            if (!user.IsBanned)
            {
                user.WarningCount = 0;
            }

            // 2. Phải bật LockoutEnabled thì Identity mới cho phép thiết lập thời gian khóa
            if (!user.LockoutEnabled)
            {
                user.LockoutEnabled = true;
            }

            // 3. Lưu cập nhật IsBanned (và LockoutEnabled, WarningCount) xuống Database TRƯỚC
            var updateResult = await _userRepository.UpdateUserAsync(user);

            // 4. Nếu lưu DB thành công, tiến hành gọi Identity để khóa/mở khóa thời gian
            if (updateResult)
            {
                if (user.IsBanned)
                {
                    await _userRepository.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue); 
                }
                else
                {
                    await _userRepository.SetLockoutEndDateAsync(user, null);
                }
            }

            return updateResult;
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
            if (user == null) return false;

            // Ưu tiên kiểm tra flag IsBanned trước, sau đó đến thời gian khóa (nếu dùng Identity Lockout)
            if (user.IsBanned) return true;

            var lockoutEndDate = await _userRepository.GetLockoutEndDateAsync(user);
            return lockoutEndDate.HasValue && lockoutEndDate > DateTimeOffset.Now;
        }
        public async Task<bool> UpdateUserByAdminAsync(UserResponse model)
        {
            var user = await _userRepository.GetByIdAsync(model.Id);
            if (user == null) return false;

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Points = model.Points;
            // Bạn có thể thêm các trường khác nếu cần

            return await _userRepository.UpdateUserAsync(user);
        }
        public async Task<IEnumerable<UserResponse>> GetReportedUsersAsync()
        {
            // Bạn đang gọi GetAllAsync() (lấy tất cả) 
            // trong khi UserRepository đã có hàm GetReportedUsersAsync() riêng biệt
            var reportedUsers = await _userRepository.GetReportedUsersAsync(); 
            return _mapper.Map<IEnumerable<UserResponse>>(reportedUsers);
        }
        // Trong file Services/Implementations/UserService.cs
        public async Task<bool> PenalizeUserAsync(string userId, int points, int warningIncrement = 1)
{
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return false;

    // 1. Trừ điểm người dùng
    user.Points -= points;
    
    // 2. TĂNG SỐ LẦN VI PHẠM (Đây là lý do Database của bạn có thể vẫn đang là 0)
    user.WarningCount += warningIncrement;

    // 3. Tự động khóa tài khoản nếu vi phạm từ 3 lần trở lên
    if (user.WarningCount >= 3)
    {
        user.IsBanned = true;
        // Dùng Identity để khóa cứng tài khoản không cho đăng nhập (khóa tới năm 2999)
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
    }

    // 4. Lưu tất cả thay đổi xuống Database
    var result = await _userManager.UpdateAsync(user);
    return result.Succeeded;
}
        public async Task<bool> AddPointsAsync(string userId, int points)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.Points += points;
            return await _userRepository.UpdateUserAsync(user);
        }
        public async Task<bool> UpdateUserRoleAsync(string userId, string targetRole, string currentUserId)
{
    var targetUser = await _userManager.FindByIdAsync(userId);
    var currentUser = await _userManager.FindByIdAsync(currentUserId);

    if (targetUser == null || currentUser == null) return false;

    // 1. Chốt chặn: Không ai được quyền hạ bệ SuperAdmin (Trùm cuối)
    if (await _userManager.IsInRoleAsync(targetUser, "SuperAdmin")) return false;

    // 2. Chốt chặn: Chỉ SuperAdmin mới có quyền nâng người khác lên Admin hoặc hạ bậc Admin
    bool isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");
    if (!isSuperAdmin) return false; 

    // 3. Xử lý thay đổi Role
    var currentRoles = await _userManager.GetRolesAsync(targetUser);
    await _userManager.RemoveFromRolesAsync(targetUser, currentRoles); // Xóa hết role cũ
    
    var result = await _userManager.AddToRoleAsync(targetUser, targetRole); // Thêm role mới
    return result.Succeeded;
}

    }
}