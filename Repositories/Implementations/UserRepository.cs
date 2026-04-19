using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.Repositories.Interfaces;

namespace StudyShare.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public UserRepository(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<IEnumerable<AppUser>> GetTopRankingAsync(int topCount)
        {
            return await _userManager.Users
                .OrderByDescending(u => u.Points)
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<AppUser?> GetByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId) ?? await _context.Users.FindAsync(userId);
        }

        public async Task<AppUser?> GetUserProfileWithIncludesAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.Documents)
                .Include(u => u.Questions)
                .Include(u => u.SavedDocuments)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> UpdateUserAsync(AppUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            if(result.Succeeded) return true;
            
            _context.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<DateTimeOffset?> GetLockoutEndDateAsync(AppUser user)
        {
            return await _userManager.GetLockoutEndDateAsync(user);
        }

        public async Task SetLockoutEndDateAsync(AppUser user, DateTimeOffset? lockoutEnd)
        {
            await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
        }

        public async Task<bool> AddSavedDocumentAsync(SavedDocument document)
        {
            _context.SavedDocuments.Add(document);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveSavedDocumentAsync(SavedDocument document)
        {
            _context.SavedDocuments.Remove(document);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<SavedDocument?> GetSavedDocumentAsync(string userId, int documentId)
        {
            return await _context.SavedDocuments
                .FirstOrDefaultAsync(s => s.UserId == userId && s.DocumentId == documentId);
        }

        public async Task<bool> IsDocumentSavedAsync(string userId, int documentId)
        {
            return await _context.SavedDocuments
                .AnyAsync(sd => sd.UserId == userId && sd.DocumentId == documentId);
        }

        public async Task<IEnumerable<SavedDocument>> GetSavedDocumentsListAsync(string userId)
        {
            return await _context.SavedDocuments
                .Where(s => s.UserId == userId)
                .Include(s => s.Document).ThenInclude(d => d.User)
                .OrderByDescending(s => s.SavedDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<AppUser>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task<IEnumerable<AppUser>> GetReportedUsersAsync()
        {
            // Lấy những User nào có danh sách Reports không trống
            return await _context.Users
                .Where(u => _context.Reports.Any(r => r.TargetUserId == u.Id))
                .ToListAsync();
        }
    }
}