using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StudyShare.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment; // Lấy đường dẫn thư mục wwwroot

        public DocumentService(AppDbContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IEnumerable<DocumentResponse>> GetAllAsync()
        {
            var docs = await _context.Documents
                .Include(d => d.Category)
                .Include(d => d.User)
                .ToListAsync();
            return _mapper.Map<IEnumerable<DocumentResponse>>(docs);
        }

        public async Task<DocumentResponse?> GetByIdAsync(int id)
        {
            var doc = await _context.Documents
                .Include(d => d.Category)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);
            return doc == null ? null : _mapper.Map<DocumentResponse>(doc);
        }

        public async Task<DocumentUpdateRequest?> GetForEditAsync(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return null;

            return new DocumentUpdateRequest
            {
                Id = doc.Id,
                Title = doc.Title,
                Description = doc.Description ?? string.Empty,
                CategoryId = doc.CategoryId
            };
        }

        public async Task<bool> UpdateAsync(DocumentUpdateRequest request, string currentUserId, bool isAdmin)
        {
            var document = await _context.Documents.FindAsync(request.Id);
            if (document == null) return false;
            if (!isAdmin && document.UserId != currentUserId) return false;

            document.Title = request.Title;
            document.Description = request.Description;
            document.CategoryId = request.CategoryId;

            if (request.File != null && request.File.Length > 0)
            {
                var oldPhysicalPath = Path.Combine(_webHostEnvironment.WebRootPath, document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPhysicalPath))
                {
                    System.IO.File.Delete(oldPhysicalPath);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(request.File.FileName);
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(fileStream);
                }

                document.FilePath = "/uploads/" + uniqueFileName;
                document.FileName = request.File.FileName;
                document.FileType = request.File.ContentType;
                document.FileSize = request.File.Length;
            }

            _context.Documents.Update(document);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CreateAsync(DocumentCreateRequest request, string userId)
        {
            var document = _mapper.Map<Document>(request);
            document.UserId = userId;
            document.UploadDate = DateTime.Now;
            document.IsApproved = false; // Mặc định phải chờ Admin duyệt

            // LOGIC XỬ LÝ FILE AN TOÀN
            if (request.File != null && request.File.Length > 0)
            {
                // Tạo tên file độc nhất để chống trùng lặp
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(request.File.FileName);
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Dùng "using" để tự động giải phóng bộ nhớ RAM sau khi lưu file xong
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(fileStream);
                }

                // Cập nhật thông tin file vào Model
                document.FilePath = "/uploads/" + uniqueFileName;
                document.FileName = request.File.FileName;
                document.FileType = request.File.ContentType;
                document.FileSize = request.File.Length;
            }

            _context.Documents.Add(document);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id, string currentUserId, bool isAdmin)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return false;

            if (!isAdmin && doc.UserId != currentUserId) return false;

            // Xóa file vật lý trong ổ cứng
            var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, doc.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            _context.Documents.Remove(doc);
            return await _context.SaveChangesAsync() > 0;
        }
                // Thêm 3 hàm này vào trong class DocumentService

        public async Task<IEnumerable<DocumentResponse>> GetAllForAdminAsync(string search)
        {
            var query = _context.Documents
                .Include(d => d.User)
                .Include(d => d.Category) // Thêm Category nếu cần hiển thị
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.Title.Contains(search));
            }

            var docs = await query.OrderByDescending(d => d.UploadDate).ToListAsync();
            return _mapper.Map<IEnumerable<DocumentResponse>>(docs);
        }

        public async Task<DocumentResponse?> GetDetailsForReviewAsync(int id)
        {
            // Lấy chi tiết để review, dùng hàm cũ GetByIdAsync cũng được vì nó đã Include Category và User
            return await GetByIdAsync(id); 
        }

        public async Task<bool> ApproveDocumentAsync(int id)
        {
            var doc = await _context.Documents.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
            if (doc == null) return false;

            if (!doc.IsApproved) 
            {
                doc.IsApproved = true;
                
                // Logic nghiệp vụ: Cộng điểm cho user được chuyển về đúng tầng Service
                if (doc.User != null)
                {
                    doc.User.Points += 10;
                }

                _context.Update(doc);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<IEnumerable<Document>> GetUserDocumentsAsync(string userId)
        {
            return await _context.Documents.Where(d => d.UserId == userId).OrderByDescending(d => d.UploadDate).ToListAsync();
        }

        public async Task<bool> DeleteByUserAsync(int id, string userId)
        {
            var doc = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
            if (doc == null) return false;

            _context.Reports.RemoveRange(_context.Reports.Where(r => r.DocumentId == id));
            _context.SavedDocuments.RemoveRange(_context.SavedDocuments.Where(s => s.DocumentId == id));

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, doc.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

            _context.Documents.Remove(doc);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}