using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
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
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public UserService(UserManager<AppUser> userManager, AppDbContext context, IWebHostEnvironment env, IMapper mapper)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
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
        public async Task<AppUser?> GetUserProfileAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.Documents).Include(u => u.Questions).Include(u => u.SavedDocuments)
                .AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, AppUser model, IFormFile avatarFile)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.FullName = model.FullName;
            user.Email = model.Email;

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var ext = Path.GetExtension(avatarFile.FileName);
                var fileName = Guid.NewGuid() + ext;
                var path = Path.Combine(_env.WebRootPath, "images", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await avatarFile.CopyToAsync(stream);
                user.Avatar = "/images/" + fileName;
            }
            _context.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SaveDocumentAsync(string userId, int docId)
        {
            if (await _context.SavedDocuments.AnyAsync(s => s.UserId == userId && s.DocumentId == docId)) return false;
            _context.SavedDocuments.Add(new SavedDocument { UserId = userId, DocumentId = docId, SavedDate = DateTime.Now });
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UnsaveDocumentAsync(string userId, int docId)
        {
            var savedDoc = await _context.SavedDocuments.FirstOrDefaultAsync(s => s.UserId == userId && s.DocumentId == docId);
            if (savedDoc == null) return false;
            _context.SavedDocuments.Remove(savedDoc);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<SavedDocument>> GetSavedDocumentsAsync(string userId)
        {
            return await _context.SavedDocuments
                .Where(s => s.UserId == userId).Include(s => s.Document).ThenInclude(d => d.User)
                .OrderByDescending(s => s.SavedDate).ToListAsync();
        }
    }
}