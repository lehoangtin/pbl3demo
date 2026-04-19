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
using System.Threading.Tasks;

namespace StudyShare.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment; // Lấy đường dẫn thư mục wwwroot

        public DocumentService(IDocumentRepository documentRepository, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _documentRepository = documentRepository;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
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

            return await _documentRepository.CreateAsync(document);
        }

        public async Task<bool> DeleteAsync(int id, string currentUserId, bool isAdmin)
        {
            var doc = await _documentRepository.GetForEditAsync(id);
            if (doc == null) return false;

            if (!isAdmin && doc.UserId != currentUserId) return false;

            // Xóa file vật lý trong ổ cứng
            var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, doc.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            return await _documentRepository.DeleteAsync(doc);
        }
                // Thêm 3 hàm này vào trong class DocumentService

        public async Task<IEnumerable<DocumentResponse>> GetAllForAdminAsync(string search)
        {
            var docs = await _documentRepository.GetAllForAdminAsync(search);
            return _mapper.Map<IEnumerable<DocumentResponse>>(docs);
        }

        public async Task<DocumentResponse?> GetDetailsForReviewAsync(int id)
        {
            // Lấy chi tiết để review, dùng hàm cũ GetByIdAsync cũng được vì nó đã Include Category và User
            return await GetByIdAsync(id); 
        }

        public async Task<bool> ApproveDocumentAsync(int id)
        {
            var doc = await _documentRepository.GetByIdAsync(id);
            if (doc == null) return false;

            return await _documentRepository.ApproveDocumentAsync(doc);
        }
        public async Task<IEnumerable<Document>> GetUserDocumentsAsync(string userId)
        {
            return await _documentRepository.GetUserDocumentsAsync(userId);
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
            
            // Ánh xạ sang DTO trả về cho Controller
            return _mapper.Map<IEnumerable<DocumentResponse>>(docs);
        }

        public async Task<DocumentResponse?> GetDocumentDetailsAsync(int id)
        {
            var document = await _documentRepository.GetDocumentDetailsAsync(id);

            return document == null ? null : _mapper.Map<DocumentResponse>(document);
        }
    }
}