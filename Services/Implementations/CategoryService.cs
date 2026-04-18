using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;

namespace StudyShare.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
        {
            var categories = await _context.Categories.Include(c => c.Documents).ToListAsync();
            return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
        }

        public async Task<CategoryResponse?> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            return category == null ? null : _mapper.Map<CategoryResponse>(category);
        }

        public async Task<CategoryUpdateRequest?> GetForEditAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            return category == null ? null : _mapper.Map<CategoryUpdateRequest>(category);
        }

        public async Task<bool> CreateAsync(CategoryCreateRequest request)
        {
            var category = _mapper.Map<Category>(request);
            _context.Categories.Add(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(CategoryUpdateRequest request)
        {
            var category = await _context.Categories.FindAsync(request.Id);
            if (category == null) return false;

            // Đổ dữ liệu từ request vào record cũ trong DB
            _mapper.Map(request, category); 
            _context.Categories.Update(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}