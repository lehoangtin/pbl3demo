using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;
using StudyShare.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudyShare.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly AppDbContext _context;
        private readonly IDocumentRepository _documentRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DocumentService(IDocumentRepository documentRepository, IUserService userService, IMapper mapper, IWebHostEnvironment webHostEnvironment, AppDbContext context)
        {
            _documentRepository = documentRepository;
            _userService = userService;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        public async Task<IEnumerable<DocumentResponse>> GetAllAsync()
        {
            var docs = await _documentRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<DocumentResponse>>(docs);
        }

        public async Task<DocumentResponse?> GetByIdAsync(int id)
        {
            var doc = await _documentRepository.GetByIdAsync(id);
            return doc == null ? null : _mapper.Map<DocumentResponse>(doc);
        }

        public async Task<DocumentUpdateRequest?> GetForEditAsync(int id)
        {
            var doc = await _documentRepository.GetForEditAsync(id);
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
            var document = await _documentRepository.GetForEditAsync(request.Id);
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

            return await _documentRepository.UpdateAsync(document);
        }

        public async Task<bool> CreateAsync(DocumentCreateRequest request, string userId)
        {
            var document = _mapper.Map<Document>(request);
            document.UserId = userId;
            document.UploadDate = DateTime.Now;
            document.IsApproved = false; // Mặc định phải chờ Admin duyệt

            if (request.File != null && request.File.Length > 0)
            {
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

            return await _documentRepository.CreateAsync(document);
        }

        public async Task<bool> DeleteAsync(int id, string currentUserId, bool isAdmin)
        {
            var doc = await _documentRepository.GetForEditAsync(id);
            if (doc == null) return false;
            if (!isAdmin && doc.UserId != currentUserId) return false;

            // Lưu đường dẫn file trước khi xóa trong DB
            var filePath = doc.FilePath; 

            var result = await _documentRepository.DeleteAsync(doc);
            if (result)
            {
                // Chỉ xóa file vật lý sau khi DB đã xóa thành công
                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            return result;
        }

        public async Task<IEnumerable<DocumentResponse>> GetAllForAdminAsync(string search)
        {
            var docs = await _documentRepository.GetAllForAdminAsync(search);
            return _mapper.Map<IEnumerable<DocumentResponse>>(docs);
        }

        public async Task<DocumentResponse?> GetDetailsForReviewAsync(int id)
        {
            return await GetByIdAsync(id); 
        }

        public async Task<bool> ApproveDocumentAsync(int id)
        {
            var doc = await _documentRepository.GetByIdAsync(id);
            if (doc == null || doc.IsApproved) return false;

            // 1. Phê duyệt tài liệu
            var success = await _documentRepository.ApproveDocumentAsync(doc);
            
            if (success)
            {
                // 2. Cộng điểm thưởng cho người đăng (ví dụ: 50 điểm)
                await _userService.AddPointsAsync(doc.UserId, 50);
            }

            return success;
        }
        public async Task<bool> IncreaseDownloadCountAsync(int id)
        {
            var doc = await _documentRepository.GetByIdAsync(id);
            if (doc == null) return false;
            
            doc.DownloadCount++;
            return await _documentRepository.UpdateAsync(doc);
        }
        public async Task<IEnumerable<DocumentResponse>> GetUserDocumentsAsync(string userId)
        {
            var docs = await _context.Documents
                .Where(d => d.UserId == userId)
                .Include(d => d.Category) // Load thêm Category để có tên danh mục
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentResponse>>(docs);
        }

        public async Task<bool> DeleteByUserAsync(int id, string userId)
        {
            var doc = await _documentRepository.GetForEditAsync(id);
            if (doc == null || doc.UserId != userId) return false;

            return await _documentRepository.DeleteByUserAsync(doc);
        }
        
        public async Task<IEnumerable<DocumentResponse>> GetApprovedDocumentsAsync(string searchTerm, int? categoryId)
        {
            var docs = await _documentRepository.GetApprovedDocumentsAsync(searchTerm, categoryId);
            return _mapper.Map<IEnumerable<DocumentResponse>>(docs);
        }

        public async Task<DocumentResponse?> GetDocumentDetailsAsync(int id)
        {
            var document = await _documentRepository.GetDocumentDetailsAsync(id);
            return document == null ? null : _mapper.Map<DocumentResponse>(document);
        }
        
        public async Task<bool> DeleteByAdminAsync(int id) 
        {
            var item = await _documentRepository.GetByIdAsync(id); 
            if (item == null) return false;
            await _documentRepository.DeleteAsync(item);
            return true;
        }
        
        public async Task<IEnumerable<DocumentResponse>> GetAllApprovedAsync()
        {
            // 🔥 Tối ưu: Dùng lại hàm GetApprovedDocumentsAsync để lấy trực tiếp từ DB
            // Truyền string rỗng và null để lấy tất cả
            var approvedDocs = await _documentRepository.GetApprovedDocumentsAsync(string.Empty, null);
            return _mapper.Map<IEnumerable<DocumentResponse>>(approvedDocs);
        }
    }
}