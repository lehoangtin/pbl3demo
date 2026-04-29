using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.Repositories.Interfaces;

namespace StudyShare.Repositories.Implementations
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _context;

        public DocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Document>> GetAllAsync()
        {
            return await _context.Documents
                .Include(d => d.Category)
                .Include(d => d.User)
                .ToListAsync();
        }

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Category)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Document?> GetForEditAsync(int id)
        {
            return await _context.Documents.FindAsync(id);
        }

        public async Task<bool> UpdateAsync(Document document)
        {
            _context.Documents.Update(document);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CreateAsync(Document document)
        {
            _context.Documents.Add(document);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Document document)
        {
            // 1. Xóa tất cả các báo cáo liên quan đến tài liệu này
            var relatedReports = _context.Reports.Where(r => r.DocumentId == document.Id);
            if (relatedReports.Any())
            {
                _context.Reports.RemoveRange(relatedReports);
            }

            // 2. Xóa tất cả các lượt "Lưu tài liệu" của người dùng khác
            var relatedSavedDocs = _context.SavedDocuments.Where(s => s.DocumentId == document.Id);
            if (relatedSavedDocs.Any())
            {
                _context.SavedDocuments.RemoveRange(relatedSavedDocs);
            }

            // 3. Sau khi dọn sạch, tiến hành xóa tài liệu
            _context.Documents.Remove(document);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Document>> GetAllForAdminAsync(string search)
        {
            var query = _context.Documents
                .Include(d => d.User)
                .Include(d => d.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.Title.Contains(search));
            }

            return await query.OrderByDescending(d => d.UploadDate).ToListAsync();
        }

        public async Task<Document?> GetDetailsForReviewAsync(int id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<bool> ApproveDocumentAsync(Document document)
        {
            if (!document.IsApproved)
            {
                document.IsApproved = true;
                _context.Update(document);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<IEnumerable<Document>> GetUserDocumentsAsync(string userId)
        {
           return await _context.Documents
                .Include(d => d.Category) 
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<bool> DeleteByUserAsync(Document document)
        {
            _context.Reports.RemoveRange(_context.Reports.Where(r => r.DocumentId == document.Id));
            _context.SavedDocuments.RemoveRange(_context.SavedDocuments.Where(s => s.DocumentId == document.Id));
            _context.Documents.Remove(document);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Document>> GetApprovedDocumentsAsync(string searchTerm, int? categoryId)
        {
            var query = _context.Documents
                .Include(d => d.Category)
                .Include(d => d.User)
                .Where(d => d.IsApproved == true);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.Title.Contains(searchTerm) || d.Description.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(d => d.CategoryId == categoryId);
            }

            return await query.OrderByDescending(d => d.UploadDate).ToListAsync();
        }

        public async Task<Document?> GetDocumentDetailsAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.User)
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
        public async Task<IEnumerable<Document>> GetPendingDocumentsAsync()
        {
            return await _context.Documents
                .Include(d => d.User)      // Để lấy tên tác giả
                .Include(d => d.Category)  // Để lấy tên danh mục
                .Where(d => d.IsApproved == false)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

    }
}